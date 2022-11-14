using System;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Helper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Trigger
{
    public class OnSessionExpireConversation : OnActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = nameof(OnSessionExpireConversation);

        [JsonProperty("ExpireAfterSeconds")]
        public int ExpireAfterSeconds { get; set; }

        [JsonConstructor]
        public OnSessionExpireConversation([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
           : base(type: ActivityHelper.SessionExpireTrigger, callerPath: callerPath, callerLine: callerLine)
        {
        
        }

        protected override Expression CreateExpression()
        {
            Type = ActivityHelper.SessionExpireTrigger;

            if (ExpireAfterSeconds > 0)
            {
                var expireSeconds = Convert.ToDouble(ExpireAfterSeconds);
                ActivityHelper.ExpireAfterSeconds = expireSeconds;
            }

            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.Type == '{this.Type}'"), base.CreateExpression());
        }
    }
}
