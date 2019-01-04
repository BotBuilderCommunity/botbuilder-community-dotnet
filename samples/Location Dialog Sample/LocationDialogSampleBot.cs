


using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.Location;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace LocationDialog_Sample
{
    public class LocationDialogSampleBot : IBot
    {
        private ConversationState _conversationState;
        private DialogSet Dialogs { get; set; }

        public LocationDialogSampleBot(ILoggerFactory loggerFactory, ConversationState conversationState)
        {
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            Dialogs = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(LocationDialogSampleBot)));

            Dialogs.Add(new LocationDialog("<BING MAPS OR AZURE MAPS API KEY>",
                    "Please enter a location",
                    conversationState,
                    useAzureMaps: true,
                    requiredFields: LocationRequiredFields.StreetAddress | LocationRequiredFields.PostalCode,
                    options: LocationOptions.None
                    ));

            Dialogs.Add(new WaterfallDialog("MainDialog", new WaterfallStep[]
            {
                async (dc, cancellationToken) =>
                {
                        return await dc.BeginDialogAsync(LocationDialog.DefaultLocationDialogId);
                },
                async (dc, cancellationToken) =>
                {
                    if (dc.Result is Place returnedPlace)
                    {
                        await dc.Context.SendActivityAsync($"Location found: {returnedPlace.GetPostalAddress().FormattedAddress}");
                    }
                    else
                    {
                        await dc.Context.SendActivityAsync($"No location found");
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
