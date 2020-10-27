using Bot.Builder.Community.Adapters.MessageBird;
using Microsoft.Extensions.Logging;

namespace MessageBird_Adapter_Sample
{
    public class MessageBirdAdapterWithErrorHandler : MessageBirdAdapter
    {
        public MessageBirdAdapterWithErrorHandler(ILogger<MessageBirdAdapter> logger, MessageBirdAdapterOptions adapterOptions)
            : base(adapterOptions, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
            };
        }
    }
}
