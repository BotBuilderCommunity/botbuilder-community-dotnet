using System;
using System.Text.Json.Serialization;

namespace Bot.Builder.Community.Adapters.RingCentral.Schema
{
    public class Params
    {
        [JsonPropertyName("author_id")]
        public string AuthorId { get; set; }
        
        [JsonPropertyName("body")]
        public string Body { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }

        [JsonPropertyName("in_reply_to_id")]
        public string InReplyToId { get; set; }

        [JsonPropertyName("thread_id")]
        public string ThreadId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedDate { get; set; }
    }
}
