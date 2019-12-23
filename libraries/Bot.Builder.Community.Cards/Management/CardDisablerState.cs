using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardDisablerState
    {
        [JsonProperty("trackedIdsByType")]
        public Dictionary<IdType, List<string>> TrackedIdsByType { get; } = new Dictionary<IdType, List<string>>();
    }
}