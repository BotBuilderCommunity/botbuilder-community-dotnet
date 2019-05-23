using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter
{

    public class NewDirectMessageObject
    {
        [JsonProperty("event")]
        public Event Event { get; set; }
    }
      
    public class Event
    {
        [JsonProperty("type")]
        public string EventType { get; set; }

        [JsonProperty("message_create")]
        public NewEvent_MessageCreate MessageCreate { get; set; }
    }

    public class NewEvent_MessageCreate
    {
        public Target target { get; set; }

        [JsonProperty("message_data")]
        public NewEvent_MessageData MessageData { get; set; }
    }
 
    public class NewEvent_MessageData
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("quick_reply")]
        public NewEvent_QuickReply QuickReply { get; set; }
    }

    public class NewEvent_QuickReply
    {
        [JsonProperty("type")]
        public string Type { get; } = "options";

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IList<NewEvent_QuickReplyOption> Options { get; set; }
    }

    public class NewEvent_QuickReplyOption
    {
        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
