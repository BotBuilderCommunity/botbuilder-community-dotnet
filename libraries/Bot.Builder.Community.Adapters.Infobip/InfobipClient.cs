using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Infobip
{
    public sealed class InfobipClient : IInfobipClient
    {
        private readonly InfobipAdapterOptions _infobipOptions;
        private readonly ILogger _logger;
        private static readonly InfobipHttpClient _infobipHttpClient;

        private const string UrlAdvancedPart = "/omni/1/advanced";

        static InfobipClient()
        {
            _infobipHttpClient = new InfobipHttpClient();
        }

        public InfobipClient(InfobipAdapterOptions infobipAdapterOptions, ILogger<InfobipClient> logger)
        {
            _infobipOptions = infobipAdapterOptions ?? throw new ArgumentNullException(nameof(infobipAdapterOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _infobipHttpClient.Init(_infobipOptions.InfobipApiKey);
        }

        public static async Task<Attachment> GetAttachmentAsync(string url, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url, UriKind.RelativeOrAbsolute),
                Method = HttpMethod.Get
            };

            var response = await _infobipHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                if (data == null) throw new Exception("Attachment was not downloaded!");
                return new Attachment{
                    Content = data,
                    ContentType = response.Content.Headers?.ContentType?.MediaType
                };
            }
            return null;
        }

        public async Task<InfobipResponseMessage> SendMessageAsync(InfobipOmniFailoverMessage infobipMessage, CancellationToken cancellationToken = default)
        {
            /* Please don't remove or change query parameters! */
            /* These parameters are used by Infobip customer care tools to identify MS Bot API requests so we could provide better and faster support to our clients. */
            var queryParams = new Dictionary<string, string>
            {
                { "piIntegrator", "89" },
                { "piPlatform", "r5rp" }
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(QueryHelpers.AddQueryString($"{_infobipOptions.InfobipApiBaseUrl}{UrlAdvancedPart}", queryParams), UriKind.RelativeOrAbsolute),
                Method = HttpMethod.Post
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var payload = infobipMessage.ToInfobipOmniFailoverOmniMessageJson();

            if (!string.IsNullOrEmpty(payload))
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var res = await _infobipHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            res.EnsureSuccessStatusCode();

            var responseBody = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            var response = responseBody.FromInfobipOmniResponseJson();
            return response?.Messages?.FirstOrDefault();
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
                var response = await _infobipHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                return response.IsSuccessStatusCode ?
                    response.Content.Headers.ContentType.MediaType :
                    null;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Content type checking failed. Message: {e.Message}");
                return null;
            }
        }
    }
}