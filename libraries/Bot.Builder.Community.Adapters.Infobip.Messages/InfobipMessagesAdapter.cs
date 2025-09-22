using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.ToActivity;
using Bot.Builder.Community.Adapters.Infobip.Messages.ToInfobip;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Messages
{
    public class InfobipMessagesAdapter : InfobipAdapterBase
    {
        private readonly InfobipMessagesAdapterOptions _adapterOptions;
        private readonly IInfobipMessagesClient _infobipMessagesClient;
        private readonly ILogger _logger;
        private readonly ToMessagesActivityConverter _toActivityConverter;
        private readonly ToInfobipMessagesConverter _toInfobipConverter;
        private readonly AuthorizationHelper _authorizationHelper;

        public InfobipMessagesAdapter(InfobipMessagesAdapterOptions adapterOptions, IInfobipMessagesClient infobipMessagesClient, ILogger<InfobipMessagesAdapter> logger)
        {
            _adapterOptions = adapterOptions ?? throw new ArgumentNullException(nameof(adapterOptions));
            _infobipMessagesClient = infobipMessagesClient ?? throw new ArgumentNullException(nameof(infobipMessagesClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _toActivityConverter = new ToMessagesActivityConverter(_adapterOptions, _infobipMessagesClient, _logger);
            _toInfobipConverter = new ToInfobipMessagesConverter(_adapterOptions, _logger);
            _authorizationHelper = new AuthorizationHelper();
        }

        /// <summary>
        /// Accepts an incoming webhook request, creates a turn context, and runs the middleware pipeline for an incoming TRUSTED activity.
        /// </summary>
        /// <param name="httpRequest">Represents the incoming side of an HTTP request.</param>
        /// <param name="httpResponse">Represents the outgoing side of an HTTP request.</param>
        /// <param name="bot">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (httpResponse == null) throw new ArgumentNullException(nameof(httpResponse));
            if (bot == null) throw new ArgumentNullException(nameof(bot));

            string stringifiedBody;

            using (var sr = new StreamReader(httpRequest.Body))
            {
                stringifiedBody = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(stringifiedBody))
            {
                _logger.LogError("Request body is empty");
                httpResponse.StatusCode = 400;
                return;
            }

            // Verify the request signature if app secret is configured
            if (!string.IsNullOrEmpty(_adapterOptions.InfobipAppSecret))
            {
                var signature = httpRequest.Headers["X-Hub-Signature"].FirstOrDefault();
                if (!_authorizationHelper.VerifySignature(signature, stringifiedBody, _adapterOptions.InfobipAppSecret))
                {
                    _logger.LogError("Request signature verification failed");
                    httpResponse.StatusCode = 401;
                    return;
                }
            }

            InfobipIncomingMessage<InfobipMessagesIncomingResult> infobipIncomingMessage;
            try
            {
                infobipIncomingMessage = JsonConvert.DeserializeObject<InfobipIncomingMessage<InfobipMessagesIncomingResult>>(stringifiedBody);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize incoming message");
                httpResponse.StatusCode = 400;
                return;
            }

            var activities = await _toActivityConverter.Convert(infobipIncomingMessage).ConfigureAwait(false);

            foreach (var activity in activities)
            {
                using (var context = new TurnContext(this, activity))
                {
                    await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                }
            }

            httpResponse.StatusCode = 200;
        }

        /// <summary>
        /// Sends activities to the conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activities">The activities to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute and if successful, an array of <see cref="ResourceResponse"/> objects containing the IDs that the receiving channel assigned to the activities.</returns>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    var response = await SendMessageActivity(turnContext, activity, cancellationToken).ConfigureAwait(false);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }
                else
                {
                    _logger.LogTrace($"Unsupported Activity Type: {activity.Type}. Only Activities of type 'Message' are supported.");
                }
            }

            return responses.ToArray();
        }

        private async Task<ResourceResponse> SendMessageActivity(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            try
            {
                var conversationId = turnContext.Activity.From.Id;
                var infobipMessage = await _toInfobipConverter.Convert(activity, conversationId).ConfigureAwait(false);

                // Priority-based API key selection
                // Priority 1: Custom API key from UI (activity properties)
                // Priority 2: Default API key from appsettings.json
                string apiKeyToUse = GetApiKeyWithPriority(activity);
                
                InfobipMessagesResponse response;
                
                if (!string.IsNullOrEmpty(apiKeyToUse) && apiKeyToUse != _adapterOptions.InfobipApiKey)
                {
                    // Use custom API key for this request
                    _logger.LogInformation($"?? Using custom API key from UI input");
                    response = await SendWithCustomApiKey(infobipMessage, apiKeyToUse, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Use default API key from configuration
                    _logger.LogInformation($"?? Using default API key from appsettings.json");
                    response = await _infobipMessagesClient.SendAsync<InfobipMessagesResponse>(infobipMessage, cancellationToken).ConfigureAwait(false);
                }

                if (response?.MessageId != null)
                {
                    return new ResourceResponse(response.MessageId);
                }

                _logger.LogWarning("Failed to send message - no message ID returned");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message activity");
                throw;
            }
        }

        /// <summary>
        /// Get API key with priority system: UI input > appsettings configuration
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <returns>API key to use</returns>
        private string GetApiKeyWithPriority(Activity activity)
        {
            // Priority 1: Custom API key from UI input (activity properties)
            if (activity.Properties != null && activity.Properties["customApiKey"] != null)
            {
                var customApiKey = activity.Properties["customApiKey"].ToString();
                if (!string.IsNullOrEmpty(customApiKey))
                {
                    _logger.LogInformation($"?? Found custom API key from UI: {customApiKey.Substring(0, Math.Min(4, customApiKey.Length))}***");
                    return customApiKey;
                }
            }

            // Priority 2: Default API key from appsettings.json
            if (!string.IsNullOrEmpty(_adapterOptions.InfobipApiKey))
            {
                _logger.LogInformation($"?? Using default API key from appsettings: {_adapterOptions.InfobipApiKey.Substring(0, Math.Min(4, _adapterOptions.InfobipApiKey.Length))}***");
                return _adapterOptions.InfobipApiKey;
            }

            // No API key found
            _logger.LogError("? No API key found! Provide API key via UI or configure InfobipApiKey in appsettings.json");
            return null;
        }

        /// <summary>
        /// Send message using a custom API key with priority system
        /// Priority 1: Custom API key from UI input (activity properties)
        /// Priority 2: Default API key from appsettings.json configuration
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="customApiKey">Custom API key to use</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from Infobip API</returns>
        private async Task<InfobipMessagesResponse> SendWithCustomApiKey(object message, string customApiKey, CancellationToken cancellationToken)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(message, Bot.Builder.Community.Adapters.Infobip.Core.InfobipSerialize.Settings);
            var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var requestUri = $"{_adapterOptions.InfobipMessagesApiBaseUrl}/messages-api/1/messages";

            // Log the request payload for debugging
            _logger.LogInformation("?? Sending message to Infobip Messages API with custom API key");
            _logger.LogInformation($"?? Endpoint: {requestUri}");
            _logger.LogInformation($"?? Request Payload: {json}");

            // Also print to console for source application debugging  
            Console.WriteLine("=== INFOBIP MESSAGES API REQUEST (CUSTOM API KEY) ===");
            Console.WriteLine($"Endpoint: {requestUri}");
            Console.WriteLine($"API Key: {customApiKey.Substring(0, Math.Min(4, customApiKey.Length))}***{customApiKey.Substring(Math.Max(0, customApiKey.Length - 4))}");
            Console.WriteLine($"Payload: {json}");
            Console.WriteLine("==================================================");

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"App {customApiKey}");

                var request = new System.Net.Http.HttpRequestMessage
                {
                    RequestUri = new Uri(requestUri),
                    Method = System.Net.Http.HttpMethod.Post,
                    Content = content
                };

                var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    
                    // Log the successful response
                    _logger.LogInformation("? Infobip Messages API response successful (custom API key)");
                    _logger.LogInformation($"?? Response Payload: {responseJson}");

                    // Also print response to console
                    Console.WriteLine("=== INFOBIP MESSAGES API RESPONSE (CUSTOM API KEY) ===");
                    Console.WriteLine($"Status: {response.StatusCode}");
                    Console.WriteLine($"Response: {responseJson}");
                    Console.WriteLine("===================================================");

                    return Newtonsoft.Json.JsonConvert.DeserializeObject<InfobipMessagesResponse>(responseJson);
                }

                var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                
                // Log the error response
                _logger.LogError("? Infobip Messages API request failed (custom API key)");
                _logger.LogError($"?? Status: {response.StatusCode}");
                _logger.LogError($"?? Error Response: {errorContent}");

                // Also print error to console
                Console.WriteLine("=== INFOBIP MESSAGES API ERROR (CUSTOM API KEY) ===");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Error: {errorContent}");
                Console.WriteLine("===============================================");

                throw new System.Net.Http.HttpRequestException($"Messages API request failed with status {response.StatusCode}: {errorContent}");
            }
        }
    }
}