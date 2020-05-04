using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class ActionsItem : BodyItem
    {
        public string Type => "actions";

        [JsonProperty(PropertyName = "items")]
        public List<ZoomAction> Actions { get; set; }
    }
}