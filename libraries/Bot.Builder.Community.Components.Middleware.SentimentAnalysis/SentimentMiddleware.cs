using Bot.Builder.Community.Components.Middleware.SentimentAnalysis.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.Middleware.SentimentAnalysis
{
    public class SentimentMiddleware : IMiddleware
    {
        private readonly ISentimentAnalysisCredentialsProvider _creds;
        public SentimentMiddleware(ISentimentAnalysisCredentialsProvider credentialsProvider)
        {
            _creds = credentialsProvider;
        }

        public string ApiKey { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is ActivityTypes.Message && _creds.IsEnabled == true)
            {
                turnContext.Activity.Conversation.Properties.Remove("Sentiment");
                turnContext.Activity.Conversation.Properties.Add("Sentiment", await turnContext.Activity.Text.Sentiment(_creds.APIKey, _creds.EndpointUrl));
            }

            await next(cancellationToken);
        }
    }
}
