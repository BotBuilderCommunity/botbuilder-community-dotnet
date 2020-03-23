using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bot.Builder.Community.Adapters.RingCentral.Schema
{
    public class Metadata
    {
        [JsonPropertyName("custom_field_values")]
        public object CustomFieldValues { get; set; }

        [JsonPropertyName("category_ids")]
        public List<string> CategoryIds { get; set; }

        [JsonPropertyName("closed_at")]
        public object ClosedDate { get; set; }

        [JsonPropertyName("deferred_at")]
        public object DeferredDate { get; set; }

        [JsonPropertyName("identity_id")]
        public string IdentityId { get; set; }

        [JsonPropertyName("source_id")]
        public string SourceId { get; set; }

        [JsonPropertyName("thread_id")]
        public string ThreadId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("approval_required")]
        public bool ApprovalRequired { get; set; }

        [JsonPropertyName("author_id")]
        public string AuthorId { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("body_input_format")]
        public string BodyInputFormat { get; set; }

        [JsonPropertyName("creator_id")]
        public object CreatorId { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("first_in_thread")]
        public bool FirstInThread { get; set; }

        [JsonPropertyName("foreign_categories")]
        public object ForeignCategories { get; set; }

        [JsonPropertyName("foreign_id")]
        public string ForeignId { get; set; }

        [JsonPropertyName("has_attachment")]
        public bool HasAttachment { get; set; }

        [JsonPropertyName("intervention_id")]
        public object InterventionId { get; set; }

        [JsonPropertyName("in_reply_to_author_id")]
        public object InReplyToAuthorId { get; set; }

        [JsonPropertyName("in_reply_to_id")]
        public object InReplyToId { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("thread_title")]
        public string ThreadTitle { get; set; }

        [JsonPropertyName("created_from")]
        public string CreatedFrom { get; set; }

        [JsonPropertyName("_private")]
        public bool Private { get; set; }

        [JsonPropertyName("context_data")]
        public ContextData ContextData { get; set; }
    }
}
