using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Bot.Builder.Community.Components.Handoff.Channel.MessageRouting;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Handoff.Channel.Action
{
    /// <summary>
    /// Custom command which takes takes 2 data bound arguments (arg1 and arg2) and multiplies them returning that as a databound result.
    /// </summary>
    public class RegisterForHandoffRequests : Dialog
    {
        [JsonConstructor]
        public RegisterForHandoffRequests([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "RegisterForHandoffRequests";

        /// <summary>
        /// Gets or sets caller's memory path to store the result of this step in (ex: conversation.area).
        /// </summary>
        /// <value>
        /// Caller's memory path to store the result of this step in (ex: conversation.area).
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageRouter = dc.Context.TurnState.Get<MessageRouter>();

            var messageRouterResult = messageRouter.CreateConnectionRequest(dc.Context.Activity.CreateSenderConversationReference(), true);

            dc.State.TryGetValue("=settings.PermittedAggregationChannels", out List<string> permittedHandoffChannels);

            var activity = dc.Context.Activity;

            bool isPermittedHandoffChannel = false;

            if (permittedHandoffChannels != null && permittedHandoffChannels.Count > 0)
            {
                foreach (string permittedHandoffChannel in permittedHandoffChannels)
                {
                    if (!string.IsNullOrWhiteSpace(activity.ChannelId)
                        && activity.ChannelId.ToLower().Equals(permittedHandoffChannel.ToLower()))
                    {
                        isPermittedHandoffChannel = true;
                        break;
                    }
                }
            }
            else
            {
                isPermittedHandoffChannel = true;
            }

            if (isPermittedHandoffChannel)
            {
                var conversationReferenceToRegister = new ConversationReference(
                    null, null, null,
                    activity.Conversation, activity.ChannelId, activity.ServiceUrl);

                var modifyRoutingDataResult = messageRouter.RegisterUserForHandoffRequests(conversationReferenceToRegister);

                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), modifyRoutingDataResult);
            }
            else
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), "NotPermitted");
            }

            return await dc.EndDialogAsync(result: messageRouterResult, cancellationToken: cancellationToken);
        }

    }
}
