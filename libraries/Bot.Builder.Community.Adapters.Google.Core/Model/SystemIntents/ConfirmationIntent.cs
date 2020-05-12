using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents
{
    public class ConfirmationIntent : SystemIntent
    {
        public ConfirmationIntent()
        {
            Intent = "actions.intent.CONFIRMATION";
        }

        public ConfirmationInputValueData InputValueData { get; set; }
    }

    public class ConfirmationInputValueData : IntentInputValueData
    {
        public ConfirmationInputValueData()
        {
            Type = "type.googleapis.com/google.actions.v2.PlaceValueSpec";
        }

        private ConfirmationIntentDialogSpec DialogSpec { get; set; }
    }

    public class ConfirmationIntentDialogSpec
    {
        public string RequestConfirmationText { get; set; }
    }
}