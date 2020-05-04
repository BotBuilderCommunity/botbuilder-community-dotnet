using Bot.Builder.Community.Adapters.Zoom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Samples.Zoom
{
    public class ZoomAdapterWithErrorHandler : ZoomAdapter
    {
        public ZoomAdapterWithErrorHandler(IConfiguration configuration, ILogger<ZoomAdapter> logger)
            : base(new ZoomAdapterOptions()
            {
                ValidateIncomingZoomRequests = false,
                ClientId = configuration["ZoomClientId"],
                ClientSecret = configuration["ZoomClientSecret"],
                BotJid = configuration["ZoomBotJid"],
                VerificationToken = configuration["ZoomVerificationToken"]
            }, logger)
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
