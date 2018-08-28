using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.Location;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Dialog_Sample
{
    public class EchoBot : IBot
    {
        private MyEchoBotAccessors _stateAccessors;

        private DialogSet dialogs;

        public EchoBot(MyEchoBotAccessors accessors)
        {
            _stateAccessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            dialogs = new DialogSet(_stateAccessors.ConversationDialogState);

            dialogs.Add(new LocationDialog(
                "Amf0lKMCmGviOMNojvzjcgdGWSZglBivKBj18hJTKGiLYxK6y52ReWaRzxgJ3xJi", 
                "Please enter a location?",
                requiredFields: LocationRequiredFields.PostalCode | LocationRequiredFields.StreetAddress | LocationRequiredFields.Region,
                useAzureMaps: false));

            var dialogCtx = await dialogs.CreateContextAsync(turnContext);

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    var results = await dialogCtx.ContinueAsync();

                    if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                    {
                        await dialogCtx.BeginAsync("LocationDialog");
                    }
                    break;
            }
        }
    }
}
