using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents
{
    public class DateTimeIntent : SystemIntent
    {
        public DateTimeIntent()
        {
            Intent = "actions.intent.DATETIME";
        }

        public DateTimeInputValueData InputValueData { get; set; }
    }

    public class DateTimeInputValueData : IntentInputValueData
    {
        public DateTimeInputValueData()
        {
            Type = "type.googleapis.com/google.actions.v2.DateTimeValueSpec";
        }

        public DateTimeIntentDialogSpec DialogSpec { get; set; }
    }

    public class DateTimeIntentDialogSpec
    {
        public string RequestDatetimeText { get; set; }
        public string RequestDateText { get; set; }
        public string RequestTimeText { get; set; }
    }
}