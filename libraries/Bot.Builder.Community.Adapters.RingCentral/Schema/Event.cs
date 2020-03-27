using System;
using System.Text.Json.Serialization;

namespace Bot.Builder.Community.Adapters.RingCentral.Schema
{
    public class Event
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("issued_at")]
        public DateTime IssuedDate { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("resource")]
        public Resource Resource { get; set; }
    }
}
