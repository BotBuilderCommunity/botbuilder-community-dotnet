using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Bot.Builder.Community.Components.Handoff.Channel.Helpers;
using Bot.Builder.Community.Components.Handoff.Channel.MessageRouting;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Handoff.Channel.Action
{
    /// <summary>
    /// Custom command which takes takes 2 data bound arguments (arg1 and arg2) and multiplies them returning that as a databound result.
    /// </summary>
    public class UnregisterForHandoffRequests : Dialog
    {
        [JsonConstructor]
        public UnregisterForHandoffRequests([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "UnregisterForHandoffRequests";

        /// <summary>
        /// Gets or sets caller's memory path to store the result of this step in (ex: conversation.area).
        /// </summary>
        /// <value>
        /// Caller's memory path to store the result of this step in (ex: conversation.area).
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var messageRouter = dc.Context.TurnState.Get<MessageRouter>();
            
            var activity = dc.Context.Activity;
            var sender = activity.CreateSenderConversationReference();

            if (messageRouter.IsRegisteredForHandoffRequests(sender))
            {
                var userToMakeUnavailable = new ConversationReference(
                    null, null, null,
                    activity.Conversation, activity.ChannelId, activity.ServiceUrl);

                var result = messageRouter.DeregisterUserForHandoffRequests(userToMakeUnavailable);

                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
                return await dc.EndDialogAsync(result, cancellationToken);
            }

            return await dc.EndDialogAsync(null, cancellationToken);
        }

    }
}
