using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Facebook.FacebookEvents.Handover
{
    /// <summary>A Facebook thread control message, including app ID of requested thread owner and an optional message
    /// to send with the request.</summary>
    public class FacebookRequestThreadControl : FacebookThreadControl
    {
        /// <summary>
        /// Gets or sets the app ID of the requested owner.
        /// </summary>
        /// <remarks>
        /// 263902037430900 for the page inbox.
        /// </remarks>
        /// <value>
        /// the app ID of the requested owner.
        /// </value>
        [JsonProperty("requested_owner_app_id")]
        public string RequestedOwnerAppId { get; set; }
    }
}
