using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;

namespace Bot.Builder.Community.Adapters.Zoom
{
    public class ZoomClient
    {
        private string _baseUrl = "https://api.zoom.us";
        private readonly string _clientId;
        private readonly string _clientSecret;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public ZoomClient(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task<IRestResponse> SendMessageAsync(ChatResponse response, CancellationToken cancellationToken)
        {
            var body = JsonConvert.SerializeObject(response, JsonSerializerSettings);
            return await SendPostRequest("/v2/im/chat/messages", body, cancellationToken);
        }

        private async Task<IRestResponse> SendPostRequest(string path, string body, CancellationToken cancellationToken)
        {
            var client = new RestClient($"{_baseUrl}{path}")
            {
                Timeout = -1,
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", $"Bearer {await GetAccessToken(cancellationToken)}");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            return await client.ExecuteAsync(request, cancellationToken);
        }

        private async Task<string> GetAccessToken(CancellationToken cancellationToken)
        {
            var client = new RestClient($"{_baseUrl}/oauth/token?grant_type=client_credentials")
            {
                Timeout = -1,
                Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret)
            };

            var request = new RestRequest(Method.POST);
            
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "", ParameterType.RequestBody);
            
            var response = await client.ExecuteAsync(request, cancellationToken);
            var tokenObject = JObject.Parse(response.Content);
            
            return tokenObject["access_token"].ToString();
        }
    }
}