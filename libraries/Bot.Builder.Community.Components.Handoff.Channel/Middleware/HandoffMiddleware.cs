using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Handoff.Channel.MessageRouting;
using Bot.Builder.Community.Components.Handoff.Channel.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.Handoff.Channel.Middleware
{
    public class HandoffMiddleware : IMiddleware
    {
        private readonly List<string> _passthroughCommands;

        public IConfiguration Configuration
        {
            get;
            protected set;
        }

        public MessageRouter MessageRouter
        {
            get;
            protected set;
        }

        public HandoffMiddleware(IConfiguration configuration, MessageRouter messageRouter)
        {
            Configuration = configuration;
            MessageRouter = messageRouter;

            _passthroughCommands = configuration.GetSection("PassthroughCommands").Get<List<string>>();
        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken ct)
        {
            var activity = context.Activity;

            if (activity.Type is ActivityTypes.Message)
            {
                // Store the conversation references (identities of the sender and the recipient [bot])
                // in the activity
                MessageRouter.StoreConversationReferences(activity);

                var sender = activity.CreateSenderConversationReference();

                if (MessageRouter.FindConnection(sender) != null
                    && MessageRouter.IsRegisteredForHandoffRequests(sender)
                    && _passthroughCommands != null && _passthroughCommands.Any()
                    && _passthroughCommands.FirstOrDefault(a => a.ToLowerInvariant() == context.Activity.Text.ToLowerInvariant()) != null)
                {
                    await next(ct).ConfigureAwait(false);
                }
                else
                {
                    // Let the message router route the activity, if the sender is connected with
                    // another user/bot
                    var messageRouterResult = await MessageRouter.RouteMessageIfSenderIsConnectedAsync(activity);

                    if (messageRouterResult != null && ((MessageRoutingResult) messageRouterResult).Type ==
                        MessageRoutingResultType.NoActionTaken)
                    {
                        await next(ct).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                // No action taken - this middleware did not consume the activity so let it propagate
                await next(ct).ConfigureAwait(false);
            }
        }
    }
}
