namespace RingCentral_Adapter_Sample
{
    using System;
    using Bot.Builder.Community.Adapters.RingCentral;
    using Bot.Builder.Community.Adapters.RingCentral.Handoff;
    using Bot.Builder.Community.Adapters.RingCentral.Middleware;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.TraceExtensions;
    using Microsoft.Extensions.Logging;

    public class RingCentralAdapterWithErrorHandler : RingCentralAdapter
    {
        public RingCentralAdapterWithErrorHandler(
            IBotFrameworkHttpAdapter botAdapter,
            RingCentralClientWrapper ringCentralClient,
            DownRenderingMiddleware downRenderingMiddleware,
            IHandoffRequestRecognizer handoffRequestRecognizer,
            ILogger<RingCentralAdapter> logger) : base(ringCentralClient, botAdapter, handoffRequestRecognizer, logger)
        {
            _ = downRenderingMiddleware ?? throw new NullReferenceException(nameof(downRenderingMiddleware));

            // Downrender outbound messages processed by the adapter
            Use(downRenderingMiddleware);

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a message to the user
                await turnContext.SendActivityAsync("The bot encountered an error or bug.");
                await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}