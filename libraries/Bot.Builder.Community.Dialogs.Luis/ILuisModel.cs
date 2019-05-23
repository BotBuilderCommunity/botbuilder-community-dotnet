using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Dialogs.Luis
{
    /// <summary>
    /// A mockable interface for the LUIS model.
    /// </summary>
    public interface ILuisModel
    {
        /// <summary>
        /// Gets the LUIS model ID.
        /// </summary>
        /// <value>
        /// The LUIS model ID.
        /// </value>
        string ModelID { get; }

        /// <summary>
        /// Gets he LUIS subscription key.
        /// </summary>
        /// <value>
        /// He LUIS subscription key.
        /// </value>
        string SubscriptionKey { get; }

        /// <summary>
        /// Gets the base Uri for accessing LUIS.
        /// </summary>
        /// <value>
        /// The base Uri for accessing LUIS.
        /// </value>
        Uri UriBase { get; }

        /// <summary>
        /// Gets the Luis Api Version.
        /// </summary>
        /// <value>
        /// The Luis Api Version.
        /// </value>
        LuisApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets the Threshold for top scoring intent
        /// </summary>
        /// <value>
        /// The Threshold for top scoring intent
        /// </value>
        double Threshold { get; }

        /// <summary>
        /// Modify a Luis request to specify query parameters like spelling or logging.
        /// </summary>
        /// <param name="request">Request so far.</param>
        /// <returns>Modified request.</returns>
        LuisRequest ModifyRequest(LuisRequest request);
    }
}
