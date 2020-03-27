// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// This is a copy/paste of PromptValidator<T> from the Bot Framework SDK
// This must be used to to internal class sealing

using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Dialogs.Prompts
{
    /// <summary>
    /// The delegate definition for custom prompt validators. Implement this function to add custom validation to a prompt.
    /// </summary>
    /// <typeparam name="T">The type the associated <see cref="Prompt{T}"/> prompts for.</typeparam>
    /// <param name="promptContext">The prompt validation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> of bool representing the asynchronous operation indicating validation success or failure.</returns>
    public delegate Task<bool> AdaptiveCardPromptValidator<T>(AdaptiveCardPromptValidatorContext<T> promptContext, CancellationToken cancellationToken);
}
