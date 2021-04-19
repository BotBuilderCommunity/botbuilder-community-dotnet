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
    public class AcceptHandoffRequest : Dialog
    {
        [JsonConstructor]
        public AcceptHandoffRequest([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "AcceptHandoffRequest";

        /// <summary>
        /// Gets or sets caller's memory path to store the result of this step in (ex: conversation.area).
        /// </summary>
        /// <value>
        /// Caller's memory path to store the result of this step in (ex: conversation.area).
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        [JsonProperty("request")]
        public StringExpression Request { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var messageRouter = dc.Context.TurnState.Get<MessageRouter>();
            var sender = dc.Context.Activity.CreateSenderConversationReference();

            var requestObject = dc.State.GetValue<ConnectionRequest>(this.Request.GetValue(dc.State));

            dc.State.TryGetValue("=settings.NoDirectConversationsWithChannels", out List<string> disallowedChannels);

            if (messageRouter.IsRegisteredForHandoffRequests(sender))
            {
                var requestorChannelAccount = new ChannelAccount(requestObject.Requestor.User.Id);
                var requestorConversationAccount = new ConversationAccount(null, null, requestObject.Requestor.Conversation.Id);
                var requestor = new ConversationReference(null, requestorChannelAccount, null, requestorConversationAccount);
                var connectionRequest = messageRouter.FindConnectionRequest(requestor);

                if (connectionRequest != null)
                {
                    Connection connection = null;

                    if (sender != null)
                    {
                        connection = messageRouter.FindConnection(sender);
                    }

                    ConversationReference senderInConnection = null;
                    ConversationReference counterpart = null;

                    if (connection?.ConversationReference1 != null)
                    {
                        if (sender.Match(connection.ConversationReference1))
                        {
                            senderInConnection = connection.ConversationReference1;
                            counterpart = connection.ConversationReference2;
                        }
                        else
                        {
                            senderInConnection = connection.ConversationReference2;
                            counterpart = connection.ConversationReference1;
                        }
                    }

                    if (senderInConnection != null)
                    {
                        // The sender (accepter/rejecter) is ALREADY connected to another party
                        var messageRouterResult = new MessageRoutingResult()
                        {
                            ErrorMessage = counterpart != null
                                ? $"Already connected to {counterpart.GetChannelAccount()?.Name}"
                                : $"Error"
                        };
                        dc.State.SetValue(this.ResultProperty.GetValue(dc.State), messageRouterResult);
                    }
                    else
                    {
                        var createNewDirectConversation = (disallowedChannels == null || !(disallowedChannels.Contains(sender.ChannelId.ToLower())));
                        var connectResult = await messageRouter.ConnectAsync(sender, connectionRequest.Requestor, createNewDirectConversation);
                        dc.State.SetValue(this.ResultProperty.GetValue(dc.State), connectResult);
                    }
                }
                else
                {
                    var messageRouterResult = new MessageRoutingResult()
                    {
                        ErrorMessage = "RequestNotFound"
                    };
                    dc.State.SetValue(this.ResultProperty.GetValue(dc.State), messageRouterResult);
                }
            }
            else
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), "NotPermitted");
            }

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}
