using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Announcer;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Middleware
{
    public class ConversationAgentMiddleware : IMiddleware
    {
        private readonly IServiceAgentAnnouncer _serviceAgentAnnouncer;

        public ConversationAgentMiddleware(IServiceAgentAnnouncer serviceAgentAnnouncer)
        {
            _serviceAgentAnnouncer = serviceAgentAnnouncer;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (ActivityHelper.IsEnable)
            {
                if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate &&
                    turnContext.Activity.MembersAdded != null)
                {
                    _serviceAgentAnnouncer.RegisterUser(turnContext);
                }
                else
                {
                    var conversationReference = turnContext.Activity.GetConversationReference();
                    _serviceAgentAnnouncer.UserLastAccessTime(conversationReference.User.Id);
                }
            }

            await next(cancellationToken);

        }
    }
}