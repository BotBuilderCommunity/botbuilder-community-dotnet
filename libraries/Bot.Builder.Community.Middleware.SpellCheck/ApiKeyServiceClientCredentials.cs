using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.SpellCheck
{
    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        string SubscriptionKey { get; set; }
        public ApiKeyServiceClientCredentials(string subscriptionKey)
        {
            SubscriptionKey = subscriptionKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
