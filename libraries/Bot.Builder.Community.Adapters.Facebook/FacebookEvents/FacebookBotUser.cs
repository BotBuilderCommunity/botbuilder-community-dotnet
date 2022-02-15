using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Represents a Facebook Bot User object.
    /// </summary>
    public class FacebookBotUser
    {
        /// <summary>
        /// Gets or sets the ID of the bot user.
        /// </summary>
        /// <value>The ID of the bot user.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
