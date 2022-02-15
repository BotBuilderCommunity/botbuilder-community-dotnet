using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Slack.Model
{
    public class ModalBlock
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "selected_option")]
        public SelectOption SelectedOption { get; set; }

        [JsonProperty(PropertyName = "selected_options")]
        public List<SelectOption> SelectedOptions { get; } = new List<SelectOption>();
    }
}
