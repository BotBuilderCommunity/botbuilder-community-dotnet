using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Middleware.SentimentAnalysis
{
    public class SentimentMiddleware : IMiddleware
    {
        public SentimentMiddleware(string apiKey)
        {
            ApiKey = apiKey;
        }

        public SentimentMiddleware() { }

        public string ApiKey { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is ActivityTypes.Message)
            {
                turnContext.TurnState.Add<string>("Sentiment", await turnContext.Activity.Text.Sentiment(ApiKey));
            }

            await next(cancellationToken);
        }
    }
}
