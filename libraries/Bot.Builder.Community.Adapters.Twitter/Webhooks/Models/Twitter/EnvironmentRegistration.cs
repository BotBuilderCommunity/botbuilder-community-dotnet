using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter
{
    public class EnvironmentRegistration
    {
        [JsonProperty("environment_name")] public string Name { get; set; }

        [JsonProperty("webhooks")] public IList<WebhookRegistration> Webhooks { get; set; }
    }
}