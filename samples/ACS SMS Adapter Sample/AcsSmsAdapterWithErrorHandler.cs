using Bot.Builder.Community.Adapters.ACS.SMS;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Samples.Google
{
    public class AcsSmsAdapterWithErrorHandler : AcsSmsAdapter
    {
        public AcsSmsAdapterWithErrorHandler(ILogger<AcsSmsAdapter> logger, AcsSmsAdapterOptions adapterOptions)
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
