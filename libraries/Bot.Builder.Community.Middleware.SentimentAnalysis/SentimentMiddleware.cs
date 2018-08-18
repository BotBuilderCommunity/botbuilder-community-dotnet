using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
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

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Activity.Type is ActivityTypes.Message)
            {
                context.Services.Add<string>(await context.Activity.Text.Sentiment(ApiKey));
            }

            await next();
        }

    }
}
