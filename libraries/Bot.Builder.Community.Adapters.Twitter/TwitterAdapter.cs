using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;

namespace Bot.Builder.Community.Adapters.Twitter
{
    public class TwitterAdapter : BotAdapter
    {
        private readonly DirectMessageSender _sender;

        public TwitterAdapter(IOptions<TwitterOptions> options)
        {
            _sender = new DirectMessageSender(options.Value);
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext,
            Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();
            foreach (var activity in activities)
            {
                await _sender.SendAsync(activity.AsTwitterMessage());
                responses.Add(new ResourceResponse(activity.Id));
            }

            return responses.ToArray();
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
