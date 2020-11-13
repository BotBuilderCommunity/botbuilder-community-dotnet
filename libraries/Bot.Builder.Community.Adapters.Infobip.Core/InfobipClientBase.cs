using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Infobip.Core
{
    public abstract class InfobipClientBase : IInfobipClientBase
    {
        private readonly InfobipAdapterOptionsBase _infobipOptions;
        private static readonly InfobipHttpClient _infobipHttpClient;

        private const string UrlAdvancedPart = "/omni/1/advanced";

        static InfobipClientBase()
        {
            _infobipHttpClient = new InfobipHttpClient();
        }

        protected InfobipClientBase(InfobipAdapterOptionsBase infobipAdapterOptions)
        {
            _infobipOptions = infobipAdapterOptions ?? throw new ArgumentNullException(nameof(infobipAdapterOptions));
            _infobipHttpClient.Init(_infobipOptions.InfobipApiKey);
        }

        public async Task<InfobipResponseMessage> SendMessageAsync(InfobipOmniFailoverMessageBase infobipMessage, CancellationToken cancellationToken = default)
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
            var payload = infobipMessage.ToJson();

            if (!string.IsNullOrEmpty(payload))
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var res = await _infobipHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            res.EnsureSuccessStatusCode();

            var responseBody = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            var response = responseBody.FromInfobipOmniResponseJson();
            return response?.Messages?.FirstOrDefault();
        }
    }
}