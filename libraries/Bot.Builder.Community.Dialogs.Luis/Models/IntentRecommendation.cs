namespace Bot.Builder.Community.Dialogs.Luis.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// LUIS intent recommendation. Look at https://www.luis.ai/Help for more
    /// information.
    /// </summary>
    public partial class IntentRecommendation
    {
        /// <summary>
        /// Initializes a new instance of the IntentRecommendation class.
        /// </summary>
        public IntentRecommendation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the IntentRecommendation class.
        /// </summary>
        public IntentRecommendation(string intent = default(string), double? score = default(double?), IList<Action> actions = default(IList<Action>))
        {
            Intent = intent;
            Score = score;
            Actions = actions;
        }

        /// <summary>
        /// Gets or sets the LUIS intent detected by LUIS service in response to a query.
        /// </summary>
        /// <value>
        /// The LUIS intent detected by LUIS service in response to a query.
        /// </value>
        [JsonProperty(PropertyName = "intent")]
        public string Intent { get; set; }

        /// <summary>
        /// Gets or sets the score for the detected intent.
        /// </summary>
        /// <value>
        /// The score for the detected intent.
        /// </value>
        [JsonProperty(PropertyName = "score")]
        public double? Score { get; set; }

        /// <summary>
        /// Gets or sets the action associated with this Luis intent.
        /// </summary>
        /// <value>
        /// The action associated with this Luis intent.
        /// </value>
        [JsonProperty(PropertyName = "actions")]
        public IList<Action> Actions { get; set; }
    }
}
