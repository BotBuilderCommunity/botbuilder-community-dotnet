using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bot.Builder.Community.Adapters.RingCentral.Schema
{
    public class Objects
    {
        [JsonPropertyName("messages")]
        public List<string> Messages { get; set; }
        
        [JsonPropertyName("private_messages")]
        public List<string> PrivateMessages { get; set; }
        
        [JsonPropertyName("threads")]
        public List<string> Threads { get; set; }
    }
}
