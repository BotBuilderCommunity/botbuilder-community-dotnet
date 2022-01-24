using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Facebook.FacebookEvents.Handover
{
    /// <summary>
    /// Event object sent to Facebook when requesting to take thread control from another app.
    /// </summary>
    public class FacebookTakeThreadControl : FacebookThreadControl
    {
        /// <summary>
        /// Gets or sets the app ID of the previous owner.
        /// </summary>
        /// <remarks>
        /// 263902037430900 for the page inbox.
        /// </remarks>
        /// <value>
        /// The app ID of the previous owner.
        /// </value>
        [JsonProperty("previous_owner_app_id")]
        public string PreviousOwnerAppId { get; set; }
    }
}
