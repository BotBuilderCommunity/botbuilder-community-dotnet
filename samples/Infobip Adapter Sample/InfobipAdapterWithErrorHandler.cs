using Bot.Builder.Community.Adapters.Infobip;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Samples.Infobip
{
    public class InfobipAdapterWithErrorHandler: InfobipAdapter
    {
        public InfobipAdapterWithErrorHandler(InfobipAdapterOptions infobipOptions, IInfobipClient infobipClient, ILogger<InfobipAdapterWithErrorHandler> logger)
            : base(infobipOptions, infobipClient, logger)
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
