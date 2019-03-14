using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bot.Builder.Community.Dialogs.ChoiceFlow
{
    public class ChoiceFlowItem
    {
        public string Name { get; set; }
        [JsonProperty("prompt")]
        public string SubChoiceFlowItemsPrompt { get; set; }
        [JsonProperty("reprompt")]
        public string SubChoiceFlowItemsRetryPrompt { get; set; }
        public List<string> Synonyms { get; set; }
        public int Id { get; set; }
        [JsonProperty("choices")]
        public List<ChoiceFlowItem> SubChoiceFlowItems { get; set; }
        public string SimpleResponse { get; set; }
    }
}
