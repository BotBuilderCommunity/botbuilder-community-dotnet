using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
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

        public new TwitterAdapter Use(IMiddleware middleware)
        {
            MiddlewareSet.Use(middleware);
            return this;
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext,
            Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();
            foreach (var activity in activities.Where(a => a.Type == ActivityTypes.Message))
            {
                await _sender.SendAsync(activity.AsTwitterMessage());
                responses.Add(new ResourceResponse(activity.Id));
            }

            return responses.ToArray();
        }

        public async Task ProcessActivity(DirectMessageEvent obj, BotCallbackHandler callback)
        {
            TurnContext context = null;

            try
            {
                var activity = RequestToActivity(obj);
                BotAssert.ActivityNotNull(activity);

                context = new TurnContext(this, activity);
                
                await RunPipelineAsync(context, callback, default).ConfigureAwait(false); 
            }
            catch (Exception ex)
            {
                await OnTurnError(context, ex);
                throw;
            }
        }

        private Activity RequestToActivity(DirectMessageEvent obj)
        {
            return new Activity
            {
                Text = obj.MessageText,
                Type = "message",
                From = new ChannelAccount(obj.Sender.Id, obj.Sender.ScreenName),
                Recipient = new ChannelAccount(obj.Recipient.Id, obj.Recipient.ScreenName),
                Conversation = new ConversationAccount { Id = obj.Sender.Id },
                ChannelId = "twitter"
            };
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
