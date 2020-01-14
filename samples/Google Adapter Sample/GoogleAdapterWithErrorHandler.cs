using Bot.Builder.Community.Adapters.Google;
using Bot.Builder.Community.Adapters.Google.Model;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Samples.Google
{
    public class GoogleAdapterWithErrorHandler : GoogleAdapter
    {
        public GoogleAdapterWithErrorHandler(ILogger<GoogleAdapter> logger, GoogleAdapterOptions adapterOptions)
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
