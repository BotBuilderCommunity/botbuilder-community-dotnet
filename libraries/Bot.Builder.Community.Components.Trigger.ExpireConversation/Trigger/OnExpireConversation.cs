using System;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Bot.Builder.Community.Components.Trigger.ExpireConversation.Middleware;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Trigger.ExpireConversation.Trigger
{
    public class OnExpireConversation : OnActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "OnExpireConversation";

        [JsonProperty("ExpireAfterSeconds")]
        public int ExpireAfterSeconds { get; set; }

        [JsonConstructor]
        public OnExpireConversation([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ConversationActivity.ActivityName, callerPath: callerPath, callerLine: callerLine)
        {
            
        }

        protected override Expression CreateExpression()
        {
            Type = ConversationActivity.ActivityName;

            if (ExpireAfterSeconds > 0)
            {
                var expireSeconds = Convert.ToDouble(ExpireAfterSeconds);
                ExpireConversationMiddleware.ExpireAfterSeconds = expireSeconds;
            }

            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.Type == '{this.Type}'"), base.CreateExpression());
        }
    }
}