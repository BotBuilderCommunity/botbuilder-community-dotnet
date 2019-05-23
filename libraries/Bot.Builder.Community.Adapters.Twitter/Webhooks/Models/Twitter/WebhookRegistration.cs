using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter
{
    public class WebhookRegistration
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("url")] public string RegisteredUrl { get; set; }

        [JsonProperty("valid")] public bool IsValid { get; set; }

        [JsonProperty("created_timestamp ")] public long CreatedTimestamp { get; set; }
    }
}
