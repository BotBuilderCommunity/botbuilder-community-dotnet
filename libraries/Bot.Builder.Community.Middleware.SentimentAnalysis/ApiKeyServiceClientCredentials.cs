using Microsoft.Rest;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.SentimentAnalysis
{
    internal class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private string SubscriptionKey;

        public ApiKeyServiceClientCredentials(string subscriptionKey)
        {
            this.SubscriptionKey = subscriptionKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
