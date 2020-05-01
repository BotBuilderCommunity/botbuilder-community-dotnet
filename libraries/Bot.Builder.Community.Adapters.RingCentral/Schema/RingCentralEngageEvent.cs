using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bot.Builder.Community.Adapters.RingCentral.Schema
{
    public class RingCentralEngageEvent : RingCentralPayload
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("domain_id")]
        public string DomainId { get; set; }

        [JsonPropertyName("events")]
        public List<Event> Events { get; set; }
    }
}
