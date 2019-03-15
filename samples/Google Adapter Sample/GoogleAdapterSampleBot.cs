using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Google_Adapter_Sample
{
    public class GoogleAdapterSampleBot : IBot
    {
        private readonly ILogger logger;

        public GoogleAdapterSampleBot(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            logger = loggerFactory.CreateLogger<GoogleAdapterSampleBot>();
            logger.LogTrace("Turn start.");
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    var activity = turnContext.Activity.CreateReply();
                    activity.Text = $"You said '{turnContext.Activity.Text}'\n";
                    activity.SuggestedActions = new SuggestedActions();
                    activity.SuggestedActions.Actions = new List<CardAction>
                    {
                        new CardAction() { Title = "Yes", Type = ActionTypes.PostBack, Value = $"yes-positive-feedback" },
                        new CardAction() { Title = "No", Type = ActionTypes.PostBack, Value = $"no-negative-feedback" }
                    };

                    turnContext.GoogleSetCard(
                        "This is the card title",
                        "This is the card subtitle",
                        new Image()
                        {
                            AccessibilityText = "This is the accessibility text",
                            Url = "https://dev.botframework.com/Client/Images/ChatBot-BotFramework.png",
                        },
                        ImageDisplayOptions.DEFAULT,
                        "This is **some text** to *go into the card*.");

                    turnContext.GoogleSetAudioResponse(
                        "http://www.hochmuth.com/mp3/Haydn_Cello_Concerto_D-1.mp3",
                        "Audio Name",
                        "This is a description of the audio",
                        new Image()
                        {
                            AccessibilityText = "This is the accessibility text",
                            Url = "https://dev.botframework.com/Client/Images/ChatBot-BotFramework.png",
                        },
                        new Image()
                        {
                            AccessibilityText = "This is the accessibility text",
                            Url = "https://dev.botframework.com/Client/Images/ChatBot-BotFramework.png",
                        });

                    await turnContext.SendActivityAsync(activity);
                    break;
            }
        }
    }
}
