﻿using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Dialogs.Prompts
{
    /// <summary>
    /// Settings to control the behavior of AdaptiveCardPrompt.
    /// </summary>
    public class AdaptiveCardPromptSettings
    {
        /// <summary>
        /// Gets or sets the card to send the user. Required.
        /// </summary>
        /// <remarks>
        /// Add the card here. Do not pass it in to `Prompt.Attachments` or it will show twice.
        /// </remarks>
        /// <value>
        /// An Adaptive Card. Can be input here or in constructor.
        /// </value>
        public Attachment Card { get; set; }

        /// <summary>
        /// Gets or sets the array of strings matching IDs of required input fields.
        /// </summary>
        /// <value>
        /// The message sent (if not null) when user uses text input instead of Adaptive Card Input.
        /// </value>
        /// <remarks>
        /// The ID strings must exactly match those used in the Adaptive Card JSON Input IDs
        /// For JSON:
        /// ```json
        /// {
        ///   "type": "Input.Text",
        ///   "id": "myCustomId",
        /// },
        /// ```
        ///   You would use `"myCustomId"` if you want that to be a required input.
        /// </remarks>
        public string[] RequiredInputIds { get; set; }

        /// <summary>
        /// Gets or sets the ID specific to this prompt.
        /// If used, you MUST add this promptId to every data.promptId in your Adaptive Card.
        /// </summary>
        /// <value>
        /// The ID specific to this prompt.
        /// </value>
        /// <remarks>
        /// Card input is only accepted if SubmitAction.data.promptId matches the promptId.
        /// Does not change between reprompts.
        /// </remarks>
        public string PromptId { get; set; }
    }
}
