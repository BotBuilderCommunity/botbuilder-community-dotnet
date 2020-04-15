using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;

namespace Bot.Builder.Community.Dialogs.Adaptive.Sql.TriggerConditions
{
    public class OnRowUpdated : OnDialogEvent
    {
        public OnRowUpdated(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base("Sql.RowUpdated", actions, condition, callerPath, callerLine)
        {
        }
    }
}
