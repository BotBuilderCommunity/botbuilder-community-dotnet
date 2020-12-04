using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Infobip.Core
{
    public sealed class InfobipHttpClient
    {
        private readonly HttpClient _httpClient;

        public InfobipHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public void Init(string apiKey)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("App", apiKey);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("You are either using an incorrect API key/access token or you are not authorized to access this API. Please contact our support https://www.infobip.com/contact.");

            return response;
        }
    }
}
