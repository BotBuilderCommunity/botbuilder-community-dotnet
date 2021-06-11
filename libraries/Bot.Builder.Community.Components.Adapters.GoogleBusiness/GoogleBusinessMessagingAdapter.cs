using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness
{
    public class GoogleBusinessMessagingAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private const string ValidateIncomingRequestsKey = "ValidateIncomingRequests";

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly GoogleBusinessMessagingAdapterOptions _options;
        private readonly ILogger _logger;
        private readonly GoogleBusinessMessagingRequestMapper _requestMapper;

        public GoogleBusinessMessagingAdapter(GoogleBusinessMessagingAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new GoogleBusinessMessagingAdapterOptions();
            _logger = logger ?? NullLogger.Instance;
            _requestMapper = new GoogleBusinessMessagingRequestMapper(new GoogleBusinessMessagingRequestMapperOptions());
        }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            string body;
            using (var sr = new StreamReader(httpRequest.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            try
            {
                var request = JsonConvert.DeserializeObject<GoogleBusinessRequest>(body, JsonSerializerSettings);
                await ProcessGoogleBusinessMessagingRequestAsync(request, bot.OnTurnAsync);
                httpResponse.ContentType = "application/json";
                httpResponse.StatusCode = (int) HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                httpResponse.ContentType = "application/json";
                httpResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        public async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic, CancellationToken cancellationToken)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (logic == null)
            {
                throw new ArgumentNullException(nameof(logic));
            }

            var request = reference.GetContinuationActivity().ApplyConversationReference(reference, true);

            using (var context = new TurnContext(this, request))
            {
                await RunPipelineAsync(context, logic, cancellationToken).ConfigureAwait(false);
            }
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(new NotImplementedException("GBM adapter does not support updateActivity."));
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            return Task.FromException(new NotImplementedException("GBM adapter does not support deleteActivity."));
        }

        private async Task ProcessGoogleBusinessMessagingRequestAsync(GoogleBusinessRequest request, BotCallbackHandler logic)
        {
            var activity = RequestToActivity(request);
            var context = new TurnContext(this, activity);
            await RunPipelineAsync(context, logic, default).ConfigureAwait(false);
        }

        public virtual Activity RequestToActivity(GoogleBusinessRequest request)
        {
            return _requestMapper.RequestToActivity(request);
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            try
            {
                var responses = new List<ResourceResponse>();

                foreach (var activity in activities)
                {
                    if (activity.Type == ActivityTypes.Message)
                    {
                        var messagingEndpoint =
                            $"https://businessmessages.googleapis.com/v1/conversations/{activity.Conversation.Id}/messages";

                        var message = _requestMapper.ActivityToMessage(activity);

                        var http = (HttpWebRequest) WebRequest.Create(new Uri(messagingEndpoint));
                        http.ContentType = "application/json";
                        http.Method = "POST";

                        string token;

                        try
                        {
                            token = await GetAccessTokenFromJsonKeyAsync(
                                _options.PathToJsonKeyFile,
                                "https://www.googleapis.com/auth/businessmessages");
                        }
                        catch (Exception ex)
                        {
                            throw new AuthenticationException("Unable to get GBM access token.", ex);
                        }

                        http.Headers.Add("Authorization", $"Bearer {token}");

                        using (var streamWriter = new StreamWriter(http.GetRequestStream()))
                        {
                            var json = JsonConvert.SerializeObject(message, JsonSerializerSettings);
                            streamWriter.Write(json);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }

                        try
                        {
                            var httpResponse = (HttpWebResponse) http.GetResponse();
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                streamReader.ReadToEnd();
                                responses.Add(new ResourceResponse(message.Name));
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new AuthenticationException("Error sending message.", ex);
                        }
                    }
                    else if (activity.Type == ActivityTypes.Command)
                    {
                        if (activity.Name == "GBMTriggerSurvey")
                        {
                            var activityValue = activity.Value as CommandValue<Guid>;
                            var surveyId = activityValue.Data.ToString();

                            var surveyEndpoint =
                                $"https://businessmessages.googleapis.com/v1/conversations/{activity.Conversation.Id}/surveys?surveyId={surveyId}";

                            var http = (HttpWebRequest)WebRequest.Create(new Uri(surveyEndpoint));
                            http.ContentType = "application/json";
                            http.Method = "POST";

                            string token;

                            try
                            {
                                token = await GetAccessTokenFromJsonKeyAsync(
                                    _options.PathToJsonKeyFile,
                                    "https://www.googleapis.com/auth/businessmessages");
                            }
                            catch (Exception ex)
                            {
                                throw new AuthenticationException("Unable to get GBM access token.", ex);
                            }

                            http.Headers.Add("Authorization", $"Bearer {token}");

                            try
                            {
                                var httpResponse = (HttpWebResponse)http.GetResponse();
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    streamReader.ReadToEnd();
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new AuthenticationException("Error sending message.", ex);
                            }
                        }
                    }
                }

                return responses.ToArray();
            }
            catch (Exception ex)
            {
                var responses = new List<ResourceResponse>();
                return responses.ToArray();
            }
        }

        /// <summary>  
        /// Get Access Token From JSON Key Async  
        /// </summary>  
        /// <param name="jsonKeyFilePath">Path to your JSON Key file</param>  
        /// <param name="scopes">Scopes required in access token</param>  
        /// <returns>Access token as string Task</returns>  
        private static async Task<string> GetAccessTokenFromJsonKeyAsync(string jsonKeyFilePath, params string[] scopes)
        {
            using (var stream = new FileStream(jsonKeyFilePath, FileMode.Open, FileAccess.Read))
            {
                return await GoogleCredential
                    .FromStream(stream) // Loads key file  
                    .CreateScoped(scopes) // Gathers scopes requested  
                    .UnderlyingCredential // Gets the credentials  
                    .GetAccessTokenForRequestAsync(); // Gets the Access Token  
            }
        }
        
        internal static bool HasConfiguration(IConfiguration configuration)
        {
            // Do we have the config needed to create an adapter?
            return !string.IsNullOrEmpty(configuration.GetValue<string>(ValidateIncomingRequestsKey));
        }
    }
}
