using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class FieldsBodyItem : BodyItem
    {
        public string Type => "fields";

        [JsonProperty(PropertyName = "items")]
        public List<ZoomField> Fields { get; set; }
    }
}