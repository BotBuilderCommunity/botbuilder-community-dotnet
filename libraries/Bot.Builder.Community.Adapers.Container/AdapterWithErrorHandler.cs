// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Alexa;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace AdapterBot
{
    public class AdapterWithErrorHandler : AlexaAdapter
    {
        private readonly ILogger<AlexaAdapter> _logger;

        public AdapterWithErrorHandler(AlexaAdapterOptions adapterOptions, ILogger<AlexaAdapter> logger)
            : base(adapterOptions, logger)
        {
            _logger = logger;

            OnTurnError = async (turnContext, exception) =>
            {
                await SendErrorMessageAsync(turnContext, exception);
            };
        }

        private async Task SendErrorMessageAsync(ITurnContext turnContext, Exception exception)
        {
            try
            {
                var errorMessageText = $"The bot encountered an error or bug. {exception.Message}. {exception.InnerException}";
                var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.IgnoringInput);
                await turnContext.SendActivityAsync(errorMessage);

                errorMessageText = "To continue to run this bot, please fix the bot source code.";
                errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(errorMessage);

                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.ToString(),
                    "https://www.botframework.com/schemas/error", "TurnError");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception caught in SendErrorMessageAsync : {ex}");
            }
        }

        public override Task<InvokeResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, Activity activity, BotCallbackHandler callback,
            CancellationToken cancellationToken)
        {
            return base.ProcessActivityAsync(claimsIdentity, activity, callback, cancellationToken);
        }
    }
}
