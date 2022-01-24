using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Facebook.FacebookEvents
{
    /// <summary>A Facebook read message, including watermark of messages that were read.</summary>
    public class FacebookRead
    {
        /// <summary>
        /// Gets or sets the timestamp were messages were read.
        /// </summary>
        /// <value>
        /// All messages that were sent before or at this timestamp were read.
        /// </value>
        [JsonProperty("watermark")]
        public long Watermark { get; set; }
    }
}
