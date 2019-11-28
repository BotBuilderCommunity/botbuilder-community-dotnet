using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Sentiment_Middleware_Sample
{
    
    public class SentimentMiddlewareBot : ActivityHandler
    {
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var sentimentAnalysisResult = (string)turnContext.TurnState["Sentiment"];

                var result = sentimentAnalysisResult;
                await turnContext.SendActivityAsync($"You said {turnContext.Activity.Text} the sentiment score according to the middleware is {(Convert.ToBoolean(result) ? "Positive" : "Negative")} ");
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync($"Error - {ex.Message} ");
            }
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to Sentiment Middleware Analysis"), cancellationToken);
                }
            }
        }
    }
}






