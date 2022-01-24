using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Slack.Model
{
    public class ModelViewState
    {
        [JsonProperty(PropertyName = "values")]
        public Dictionary<string, Dictionary<string, ModalBlock>> Values { get; } = new Dictionary<string, Dictionary<string, ModalBlock>>();
    }
}
