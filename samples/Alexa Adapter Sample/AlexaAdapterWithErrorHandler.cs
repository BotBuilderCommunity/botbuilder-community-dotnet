using Bot.Builder.Community.Adapters.Alexa;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Samples.Google
{
    public class AlexaAdapterWithErrorHandler : AlexaAdapter
    {
        public AlexaAdapterWithErrorHandler(ILogger<AlexaAdapter> logger)
            : base(new AlexaAdapterOptions(), logger)
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
