using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bot.Builder.Community.Adapters.RingCentral.Schema
{
    public class ImplementationInfo
    {
        [JsonPropertyName("objects")]
        public Objects Objects { get; set; }
        
        [JsonPropertyName("options")]
        public List<string> Options { get; set; }
    }

}
