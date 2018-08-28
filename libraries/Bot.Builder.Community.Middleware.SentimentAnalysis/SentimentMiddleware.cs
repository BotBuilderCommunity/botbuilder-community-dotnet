using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.SentimentAnalysis
{
    public class SentimentMiddleware : IMiddleware
    {
        public SentimentMiddleware(IConfiguration configuration)
        {
            ApiKey = configuration.GetValue<string>("SentimentKey");
        }

        public string ApiKey { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is ActivityTypes.Message)
            {
                turnContext.TurnState.Add<string>(await turnContext.Activity.Text.Sentiment(ApiKey));
            }

            await next(cancellationToken);
        }
    }
}
