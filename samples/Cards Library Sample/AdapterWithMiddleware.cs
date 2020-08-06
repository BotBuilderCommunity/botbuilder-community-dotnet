using Bot.Builder.Community.Cards.Management;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cards_Library_Sample
{
    public class AdapterWithMiddleware : BotFrameworkHttpAdapter
    {
        public AdapterWithMiddleware(
                IConfiguration configuration,
                InspectionState inspectionState,
                ConversationState conversationState,
                CardManager cardManager,
                ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            // Inspection needs credentials because it will be sending the Activities and User and Conversation State to the emulator
            var credentials = new MicrosoftAppCredentials(configuration["MicrosoftAppId"], configuration["MicrosoftAppPassword"]);

            Use(new InspectionMiddleware(inspectionState, null, conversationState, credentials));

            var cardManagerMiddleware = new CardManagerMiddleware(cardManager);

            cardManagerMiddleware.NonUpdatingOptions.AutoClearEnabledOnSend = false;

            Use(cardManagerMiddleware
                .SetAutoApplyIds(false)
                .SetIdOptions(new DataIdOptions(new[]
                {
                    DataIdScopes.Action,
                    DataIdScopes.Card,
                    DataIdScopes.Carousel,
                    DataIdScopes.Batch,
                })));

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
