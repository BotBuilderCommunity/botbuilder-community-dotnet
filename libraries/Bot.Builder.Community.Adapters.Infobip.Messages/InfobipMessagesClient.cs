using Bot.Builder.Community.Adapters.Infobip.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Infobip.Messages
{
    public sealed class InfobipMessagesClient : IInfobipMessagesClient
    {
        private readonly InfobipMessagesAdapterOptions _adapterOptions;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public InfobipMessagesClient(InfobipMessagesAdapterOptions adapterOptions, HttpClient httpClient = null, ILogger<InfobipMessagesClient> logger = null)
        {
            _adapterOptions = adapterOptions ?? throw new ArgumentNullException(nameof(adapterOptions));
            _httpClient = httpClient ?? new HttpClient();
            _logger = logger;

            // Set up authentication header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"App {_adapterOptions.InfobipApiKey}");
        }

        public async Task<Attachment> GetAttachmentAsync(string url, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url, UriKind.RelativeOrAbsolute),
                Method = HttpMethod.Get
            };

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                if (data == null) throw new Exception("Attachment was not downloaded!");
                return new Attachment
                {
                    Content = data,
                    ContentType = response.Content.Headers?.ContentType?.MediaType
                };
            }
            return null;
        }

        public async Task<string> GetContentTypeAsync(string url, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url, UriKind.RelativeOrAbsolute),
                Method = HttpMethod.Head
            };

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                return response.IsSuccessStatusCode ?
                    response.Content.Headers.ContentType.MediaType :
                    null;
            }
            catch (Exception e)
            {
                _logger?.LogWarning($"Content type checking failed. Message: {e.Message}");
                return null;
            }
        }

        public async Task<T> SendAsync<T>(object message, CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(message, InfobipSerialize.Settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestUri = $"{_adapterOptions.InfobipMessagesApiBaseUrl}/messages-api/1/messages";

            // Log the request payload for debugging
            _logger?.LogInformation("?? Sending message to Infobip Messages API");
            _logger?.LogInformation($"?? Endpoint: {requestUri}");
            _logger?.LogInformation($"?? Request Payload: {json}");

            // Also print to console for source application debugging
            Console.WriteLine("=== INFOBIP MESSAGES API REQUEST ===");
            Console.WriteLine($"Endpoint: {requestUri}");
            Console.WriteLine($"Payload: {json}");
            Console.WriteLine("===================================");

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(requestUri),
                Method = HttpMethod.Post,
                Content = content
            };

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                
                // Log the successful response
                _logger?.LogInformation("? Infobip Messages API response successful");
                _logger?.LogInformation($"?? Response Payload: {responseJson}");

                // Also print response to console
                Console.WriteLine("=== INFOBIP MESSAGES API RESPONSE ===");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Response: {responseJson}");
                Console.WriteLine("====================================");

                return JsonConvert.DeserializeObject<T>(responseJson);
            }

            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            // Log the error response
            _logger?.LogError("? Infobip Messages API request failed");
            _logger?.LogError($"?? Status: {response.StatusCode}");
            _logger?.LogError($"?? Error Response: {errorContent}");

            // Also print error to console
            Console.WriteLine("=== INFOBIP MESSAGES API ERROR ===");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Error: {errorContent}");
            Console.WriteLine("=================================");

            throw new HttpRequestException($"Messages API request failed with status {response.StatusCode}: {errorContent}");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}