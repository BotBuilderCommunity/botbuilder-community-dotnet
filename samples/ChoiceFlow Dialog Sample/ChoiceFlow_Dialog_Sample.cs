using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.ChoiceFlow;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ChoiceFlow_Dialog_Sample
{
    public class EmptyBot : ActivityHandler
    {
        private ConversationState _conversationState;
        private DialogSet Dialogs { get; set; }

        public EmptyBot(ILoggerFactory loggerFactory, ConversationState conversationState)
        {

            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            Dialogs = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(ChoiceFlow_Dialog_Sample)));

            var pathToChoiceFlowJson = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "choiceFlow.json");

            Dialogs.Add(new ChoiceFlowDialog(pathToChoiceFlowJson));
            Dialogs.Add(new WaterfallDialog("MainDialog", new WaterfallStep[]
              {
                async (dc, cancellationToken) =>
                {
                        return await dc.BeginDialogAsync(ChoiceFlowDialog.DefaultDialogId);
                },
                async (dc, cancellationToken) =>
                {
                    if (dc.Result is ChoiceFlowItem returnedItem)
                    {
                        await dc.Context.SendActivityAsync($"The choice flow has finished. The user picked {returnedItem.Name}");
                    }

                    return await dc.EndDialogAsync();
                }
              }));
        }


        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            var dc = await Dialogs.CreateContextAsync(turnContext);
            var dialogResult = await dc.ContinueDialogAsync();

            if (!dc.Context.Responded)
            {
                switch (dialogResult.Status)
                {
                    case DialogTurnStatus.Empty:
                        await dc.BeginDialogAsync("MainDialog");
                        break;

                    case DialogTurnStatus.Waiting:
                        break;

                    case DialogTurnStatus.Complete:
                        await dc.EndDialogAsync();
                        break;

                    default:
                        await dc.CancelAllDialogsAsync();
                        break;
                }
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello world!"), cancellationToken);
                }
            }
        }
    }
}
