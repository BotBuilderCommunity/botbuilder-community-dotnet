


using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.ChoiceFlow;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ChoiceFlowDialog_Sample
{
    public class ChoiceFlowDialog_SampleBot : IBot
    {
        private ConversationState _conversationState;
        private DialogSet Dialogs { get; set; }

        public ChoiceFlowDialog_SampleBot(ILoggerFactory loggerFactory, ConversationState conversationState)
        {
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            Dialogs = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(ChoiceFlowDialog_SampleBot)));

            Dialogs.Add(new ChoiceFlowDialog("choiceFlow.json"));

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

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dc = await Dialogs.CreateContextAsync(turnContext);

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
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
                    break;
            }
        }
    }
}
