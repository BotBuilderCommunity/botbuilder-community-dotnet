// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
                    await turnContext.SendActivityAsync(activity);                    
                    break;
            }
        }
    }
}
