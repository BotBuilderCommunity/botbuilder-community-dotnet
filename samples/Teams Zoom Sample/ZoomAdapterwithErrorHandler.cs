using Bot.Builder.Community.Adapters.Zoom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teams_Zoom_Sample
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
