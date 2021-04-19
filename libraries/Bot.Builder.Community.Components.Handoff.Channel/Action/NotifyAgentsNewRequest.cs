using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Bot.Builder.Community.Components.Handoff.Channel.MessageRouting;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Handoff.Channel.Action
{
    /// <summary>
    /// Custom command which takes takes 2 data bound arguments (arg1 and arg2) and multiplies them returning that as a databound result.
    /// </summary>
    public class NotifyAgentsNewRequest : Dialog
    {
        [JsonConstructor]
        public NotifyAgentsNewRequest([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
            this.Activity = new ActivityTemplate(string.Empty);
        }

        [JsonProperty("$kind")]
        public const string Kind = "NotifyAgentsNewRequest";

        [JsonProperty("activity")]
        public ITemplate<Activity> Activity { get; set; }

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
            var activity = await Activity.BindAsync(dc, dc.State).ConfigureAwait(false);

            foreach (ConversationReference aggregationChannel
                in messageRouter.GetUsersRegisteredForHandoffRequests())
            {
                ConversationReference botConversationReference =
                    messageRouter.FindConversationReference(
                        aggregationChannel.ChannelId, aggregationChannel.Conversation.Id, null, true);

                if (botConversationReference != null)
                {

                    if (activity.Type != "message"
                        || !string.IsNullOrEmpty(activity.Text)
                        || activity.Attachments?.Any() == true
                        || !string.IsNullOrEmpty(activity.Speak)
                        || activity.SuggestedActions != null
                        || activity.ChannelData != null)
                    {
                        await messageRouter.SendMessageAsync(aggregationChannel, activity);
                    }
                }
            }

            return await dc.EndDialogAsync(result: null, cancellationToken: cancellationToken);
        }
    }
}
