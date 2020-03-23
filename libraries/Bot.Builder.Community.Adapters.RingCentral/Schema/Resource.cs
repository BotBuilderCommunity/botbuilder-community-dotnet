using System.Text.Json.Serialization;

namespace Bot.Builder.Community.Adapters.RingCentral.Schema
{
    public class Resource
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }
    }
}
