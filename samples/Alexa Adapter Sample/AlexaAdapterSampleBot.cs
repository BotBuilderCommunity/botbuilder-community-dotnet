using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Alexa;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace AlexaAdapter_Sample
{
    public class AlexaAdapterSampleBot : IBot
    {
        private readonly ILogger logger;

        public AlexaAdapterSampleBot(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            logger = loggerFactory.CreateLogger<AlexaAdapterSampleBot>();
            logger.LogTrace("Turn start.");
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    if (turnContext.Activity.Text == "AMAZON.CancelIntent")
                    {
                        await turnContext.SendActivityAsync("You asked to cancel!");
                    }
                    else
                    {
                        await turnContext.SendActivityAsync($"You said '{turnContext.Activity.Text}'\n");

                        turnContext.AlexaSetCard(new AlexaCard()
                        {
                            Type = AlexaCardType.Simple,
                            Title = "Alexa Card Sample",
                            Content = $"You said '{turnContext.Activity.Text}'\n",
                        });
                    }

                    break;
                case AlexaRequestTypes.LaunchRequest:
                    var responseMessage = $"You launched the Alexa Bot Sample!";
                    await turnContext.SendActivityAsync(responseMessage);
                    break;
            }
        }
    }
}
