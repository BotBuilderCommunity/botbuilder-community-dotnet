using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Represents the response object received from Facebook API when a message is sent.
    /// </summary>
    public class FacebookResponseOk
    {
        /// <summary>
        /// Gets or sets the recipient ID.
        /// </summary>
        /// <value>The ID of the recipient.</value>
        [JsonProperty(PropertyName = "recipient_id")]
        public string RecipientId { get; set; }

        /// <summary>
        /// Gets or sets the message ID.
        /// </summary>
        /// <value>The message ID.</value>
        [JsonProperty(PropertyName = "message_id")]
        public string MessageId { get; set; }
    }
}
