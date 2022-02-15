using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Slack.Model.Events
{
    /// <summary>
    /// Represents a Slack Message event https://api.slack.com/events/message.
    /// </summary>
    public class MessageEvent : EventType
    {
        public string Text { get; set; }

        [JsonProperty(PropertyName = "channel_type")]
        public string ChannelType { get; set; }

        public string SubType { get; set; }
    }
}
