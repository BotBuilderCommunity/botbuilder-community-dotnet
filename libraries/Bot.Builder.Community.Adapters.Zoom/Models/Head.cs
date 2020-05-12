using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class Head
    {
        public string Text { get; set; }

        [JsonProperty(PropertyName = "sub_head")]
        public SubHead SubHead { get; set; }

        public Style Style { get; set; }
    }
}