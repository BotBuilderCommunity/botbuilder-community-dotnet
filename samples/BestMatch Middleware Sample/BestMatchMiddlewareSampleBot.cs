// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace LocationDialog_Sample
{
    public class BestMatchMiddlewareSampleBot : IBot
    {
        private readonly ILogger logger;

        public BestMatchMiddlewareSampleBot(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            logger = loggerFactory.CreateLogger<BestMatchMiddlewareSampleBot>();
            logger.LogTrace("Turn start.");
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                        await turnContext.SendActivityAsync($"You said '{turnContext.Activity.Text}'\n");
                    break;
            }
        }
    }
}
