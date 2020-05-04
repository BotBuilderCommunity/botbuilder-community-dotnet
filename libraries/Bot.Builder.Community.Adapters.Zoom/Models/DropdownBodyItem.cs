using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class DropdownBodyItem : BodyItem
    {
        public string Type => "select";

        public string Text { get; set; }

        [JsonProperty(PropertyName = "select_items")]
        public List<ZoomSelectItem> SelectItems { get; set; }

        [JsonProperty(PropertyName = "selected_item")]
        public ZoomSelectItem SelectedItem { get; set; }
    }
}