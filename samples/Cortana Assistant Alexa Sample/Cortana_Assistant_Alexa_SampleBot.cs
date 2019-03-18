// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Alexa;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using System.Linq;
using Cortana_Assistant_Alexa_Sample.Models;
using Cortana_Assistant_Alexa_Sample.Helpers;
using Bot.Builder.Community.Adapters.Google;

namespace Cortana_Assistant_Alexa_Sample
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class Cortana_Assistant_Alexa_SampleBot : IBot
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="conversationState">The managed conversation state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public Cortana_Assistant_Alexa_SampleBot(ConversationState conversationState, ILoggerFactory loggerFactory)
        {
            if (conversationState == null)
            {
                throw new System.ArgumentNullException(nameof(conversationState));
            }

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<Cortana_Assistant_Alexa_SampleBot>();
            _logger.LogTrace("Turn start.");
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // If the request is from Google or any out of the box channels
                Activity activity = turnContext.Activity.CreateReply();
                string carat = new string(turnContext.Activity.Text?.Where(Char.IsDigit).ToArray());

                await SendActivity(turnContext, carat);
            }
            else if(turnContext.Activity.Type == "IntentRequest") // If the request is from Alexa.
            {
               
                AlexaIntent alexaIntent = (AlexaIntent)turnContext.Activity.Value;
                switch(alexaIntent.Name)
                {
                    case "GetRates":
                        // Extracting the entities (slots) and their values
                        string slot = alexaIntent.Slots["CaratType"]?.Value;
                        string carat = new string(slot?.Where(Char.IsDigit).ToArray());
                        await SendActivity(turnContext, carat);
                        break;
                    default:
                        // If it's none (default callback)
                        await SendActivity(turnContext, "None");
                        break;
                }                             
            }
        }

        // Send Activity to channels
        private async Task SendActivity(ITurnContext turnContext, string message)
        {
            Activity activity = turnContext.Activity.CreateReply();

            if (message == "None")
                activity.Text = Messages.GetNoneMessages();
            else
            {
                // Fetching the gold rates
                GoldRate result = GoldRatesParser.GetRates().Result;

                // Messages based upon the slot's value
                if (string.IsNullOrEmpty(message))
                    activity.Text = string.Format(Messages.GetRateMessages(), result.Carat24, result.Carat22);
                else if (message == "22")
                    activity.Text = string.Format(Messages.GetRateCaratMessages(), message, result.Carat22);
                else
                    activity.Text = string.Format(Messages.GetRateCaratMessages(), message, result.Carat24);
            }

            // Just to cater for google. 
            if(turnContext.Activity.ChannelId == "google")
            {

                var card = new GoogleBasicCard()
                {
                    Content = new GoogleBasicCardContent()
                    {
                        Title = "Today's gold rate",
                        Subtitle = "Keeping you up to date about the gold rates on daily basis.",
                        FormattedText = activity.Text,
                        Display = ImageDisplayOptions.DEFAULT,
                        Image = new Image()
                        {
                            Url = "https://images-na.ssl-images-amazon.com/images/I/71lpB+tqfgL._SL210_QL95_BG0,0,0,0_FMpng_.png"
                        },
                    },
                };

                turnContext.GoogleSetCard(card);
            }

            await turnContext.SendActivityAsync(activity);
        }
    }
}
