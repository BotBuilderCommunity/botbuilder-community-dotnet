namespace Bot.Builder.Community.Dialogs.Luis
{
    using Bot.Builder.Community.Dialogs.Luis.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// The schema for the LUIS trace info
    /// </summary>
    public class LuisTraceInfo
    {
        /// <summary>
        /// Gets or sets the raw response coming back from the LUIS service
        /// </summary>
        /// <value>
        /// The raw response coming back from the LUIS service
        /// </value>
        [JsonProperty("luisResult")]
        public LuisResult LuisResult { get; set; }

        /// <summary>
        /// Gets or sets the options passed to the LUIS service
        /// </summary>
        /// <value>
        /// The options passed to the LUIS service
        /// </value>
        [JsonProperty("luisOptions")]
        public ILuisOptions LuisOptions { get; set; }

        /// <summary>
        /// Gets or sets the metadata about the LUIS app
        /// </summary>
        /// <value>
        /// The metadata about the LUIS app
        /// </value>
        [JsonProperty("luisModel")]
        public ILuisModel LuisModel { get; set; }
    }
}
