using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardDisablerState
    {
        [JsonProperty("disabledIdsByType")]
        public Dictionary<IdType, List<string>> DisabledIdsByType { get; } = new Dictionary<IdType, List<string>>();
    }
}