using Bot.Builder.Community.Adapters.Infobip.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp
{
    public sealed class InfobipWhatsAppClient : InfobipClientBase, IInfobipWhatsAppClient
    {
        private readonly InfobipWhatsAppAdapterOptions _infobipWhatsAppOptions;
        private readonly ILogger _logger;
        private static readonly InfobipHttpClient _infobipHttpClient;

        static InfobipWhatsAppClient()
        {
            _infobipHttpClient = new InfobipHttpClient();
        }

        public InfobipWhatsAppClient(InfobipWhatsAppAdapterOptions infobipWhatsAppAdapterOptions, ILogger<InfobipWhatsAppClient> logger):base(infobipWhatsAppAdapterOptions)
        {
            _infobipWhatsAppOptions = infobipWhatsAppAdapterOptions ?? throw new ArgumentNullException(nameof(infobipWhatsAppAdapterOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _infobipHttpClient.Init(_infobipWhatsAppOptions.InfobipApiKey);
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