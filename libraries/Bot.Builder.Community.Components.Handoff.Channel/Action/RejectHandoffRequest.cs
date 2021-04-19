using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Bot.Builder.Community.Components.Handoff.Channel.Helpers;
using Bot.Builder.Community.Components.Handoff.Channel.MessageRouting;
using Bot.Builder.Community.Components.Handoff.Channel.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Handoff.Channel.Action
{
    /// <summary>
    /// Custom command which takes takes 2 data bound arguments (arg1 and arg2) and multiplies them returning that as a databound result.
    /// </summary>
    public class RejectHandoffRequest : Dialog
    {
        [JsonConstructor]
        public RejectHandoffRequest([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "RejectHandoffRequest";

        /// <summary>
        /// Gets or sets caller's memory path to store the result of this step in (ex: conversation.area).
        /// </summary>
        /// <value>
        /// Caller's memory path to store the result of this step in (ex: conversation.area).
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        [JsonProperty("request")]
        public ObjectExpression<ConnectionRequest> Request { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageRouter = dc.Context.TurnState.Get<MessageRouter>();
            var activity = dc.Context.Activity;
            var sender = activity.CreateSenderConversationReference();

            dc.State.TryGetValue("=settings.NoDirectConversationsWithChannels", out List<string> disallowedChannels);

            if (messageRouter.IsRegisteredForHandoffRequests(sender))
            {
                var requestorChannelAccount = new ChannelAccount(Request.Value.Requestor.User.Id);
                var requestorConversationAccount = new ConversationAccount(null, null, Request.Value.Requestor.Conversation.Id);

                var messageRouterResult = new ConnectionRequestResult()
                {
                    Type = ConnectionRequestResultType.Error
                };

                var requestor = new ConversationReference(null, requestorChannelAccount, null, requestorConversationAccount);
                var connectionRequest = messageRouter.FindConnectionRequest(requestor);

                if (connectionRequest != null)
                {
                    // Note: Rejecting is OK even if the sender is alreay connected
                    messageRouterResult = messageRouter.RejectConnectionRequest(connectionRequest.Requestor, sender);
                }
                else
                {
                    messageRouterResult.ErrorMessage = "RequestNotFound";
                }

                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), messageRouterResult);
            }
            else
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), "NotPermitted");
            }

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}
