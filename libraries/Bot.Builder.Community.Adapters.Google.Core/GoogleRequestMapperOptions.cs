using Bot.Builder.Community.Adapters.Google.Core.Model;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    public class GoogleRequestMapperOptions
    {
        public string ChannelId { get; set; } = "google";
        public string ServiceUrl { get; set; } = null;
        public bool ShouldEndSessionByDefault { get; set; } = true;
        public string ActionInvocationName { get; set; } = string.Empty;
        public GoogleWebhookType WebhookType { get; set; }
    }
}
