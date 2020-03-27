﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// This is a copy/paste of PromptValidatorContext<T> from the Bot Framework SDK
// This must be used to to internal class sealing

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Builder.Community.Dialogs.Prompts
{
    /// <summary>
    /// Contains context information for a <see cref="PromptValidator{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value the prompt returns.</typeparam>
    public class AdaptiveCardPromptValidatorContext<T>
    {
        internal AdaptiveCardPromptValidatorContext(ITurnContext turnContext, PromptRecognizerResult<T> recognized, IDictionary<string, object> state, PromptOptions options)
        {
            Context = turnContext;
            Options = options;
            Recognized = recognized;
            State = state;
        }

        /// <summary>
        /// Gets the <see cref="TurnContext"/> for the current turn of conversation with the user.
        /// </summary>
        /// <value>Context for the current turn of conversation with the user.</value>
        public ITurnContext Context { get; }

        /// <summary>
        /// Gets the <see cref="PromptRecognizerResult{T}"/> returned from the prompt's recognition attempt.
        /// </summary>
        /// <value>The recognition results from the prompt's recognition attempt.</value>
        public PromptRecognizerResult<T> Recognized { get; }

        /// <summary>
        /// Gets the <see cref="PromptOptions"/> used for this recognition attempt.
        /// </summary>
        /// <value>The prompt options used for this recognition attempt.</value>
        public PromptOptions Options { get; }

        /// <summary>
        /// Gets state for the associated prompt instance.
        /// </summary>
        /// <value>State for the associated prompt instance.</value>
        public IDictionary<string, object> State { get; }

        /// <summary>
        /// Gets the number of times this instance of the prompt has been executed.
        /// </summary>
        /// <value>A number indicating how many times the prompt was invoked (starting at 1 for the first time it was called).</value>
        /// <remarks>This count is set when the prompt is added to the dialog stack.</remarks>
        public int AttemptCount
        {
            get
            {
                if (!State.ContainsKey(AdaptiveCardPrompt.AttemptCountKey))
                {
                    return 0;
                }

                return Convert.ToInt32(State[AdaptiveCardPrompt.AttemptCountKey]);
            }
        }
    }
}
