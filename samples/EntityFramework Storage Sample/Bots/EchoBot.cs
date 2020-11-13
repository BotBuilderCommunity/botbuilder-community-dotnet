using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EntityFrameworkTranscriptStoreExample.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly IStatePropertyAccessor<int> _totalMessagesProperty;

        public EchoBot(ConversationState conversationState)
        {
            _conversationState = conversationState;
            // Create the property accessor used for tracking # of messages a user sent per conversation.
            _totalMessagesProperty = _conversationState.CreateProperty<int>(nameof(_totalMessagesProperty));
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var messageTotal = await _totalMessagesProperty.GetAsync(turnContext, () => 0, cancellationToken) + 1;
            await turnContext.SendActivityAsync(MessageFactory.Text($"{messageTotal} Echo: {turnContext.Activity.Text}"), cancellationToken);

            await _totalMessagesProperty.SetAsync(turnContext, messageTotal, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}
