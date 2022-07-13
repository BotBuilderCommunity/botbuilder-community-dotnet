using System;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Helper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Trigger
{
    public class OnReminderConversation : OnActivity
    {
        [JsonProperty("$kind")] 
        public new const string Kind = nameof(OnReminderConversation);

        [JsonProperty("ReminderAfterSeconds")]
        public int Reminder { get; set; }

        [JsonConstructor]
        public OnReminderConversation([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityHelper.ReminderTrigger, callerPath: callerPath, callerLine: callerLine)
        {

        }

        protected override Expression CreateExpression()
        {
            Type = ActivityHelper.ReminderTrigger;

            if (Reminder > 0)
            {
                var reminder = Convert.ToDouble(Reminder);
                ActivityHelper.ReminderSeconds = reminder;
            }

            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.Type == '{this.Type}'"),
                base.CreateExpression());
        }
    }
}