using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter
{
    public class WebhookResult
    {
        [JsonProperty("environments")] public IList<EnvironmentRegistration> Environments { get; set; }
    }
}