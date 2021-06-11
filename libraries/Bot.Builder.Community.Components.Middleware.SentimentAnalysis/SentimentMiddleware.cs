using Bot.Builder.Community.Components.Middleware.SentimentAnalysis.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.Middleware.SentimentAnalysis
{
    public class SentimentMiddleware : IMiddleware
    {
        private readonly ISentimentAnalysisCredentialsProvider _creds;
        private const string Turn = "turn";

        public SentimentMiddleware(ISentimentAnalysisCredentialsProvider credentialsProvider)
        {
            _creds = credentialsProvider;
        }

        public string ApiKey { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is ActivityTypes.Message && _creds.IsEnabled == true)
            {
                ObjectPath.SetPathValue(turnContext.TurnState, "turn.Sentiment", await turnContext.Activity.Text.Sentiment(_creds.APIKey, _creds.EndpointUrl));
            }

            await next(cancellationToken);
        }
    }
}
