using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Slack.Model
{
    public class SelectOption
    {
        [JsonProperty(PropertyName = "text")]
        public object Text { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
