// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.ChoiceFlow;
using Bot.Builder.Community.Dialogs.Location;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ChoiceFlowDialogSample
{
    public class ChoiceFlowDialogSampleBot : IBot
    {
        private ConversationState _conversationState;
        private DialogSet Dialogs { get; set; }

        public ChoiceFlowDialogSampleBot(ILoggerFactory loggerFactory, ConversationState conversationState)
        {
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            Dialogs = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(ChoiceFlowDialogSampleBot)));

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
