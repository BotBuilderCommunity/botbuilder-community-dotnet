using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Bot.Builder.Community.Components.Handoff.Channel.Helpers;
using Bot.Builder.Community.Components.Handoff.Channel.MessageRouting;
using Bot.Builder.Community.Components.Handoff.Channel.Models;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Handoff.Channel.Action
{
    /// <summary>
    /// Custom command which takes takes 2 data bound arguments (arg1 and arg2) and multiplies them returning that as a databound result.
    /// </summary>
    public class CreateHandoffRequest : Dialog
    {
        [JsonConstructor]
        public CreateHandoffRequest([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "CreateHandoffRequest";

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

            var messageRouterResult = messageRouter.CreateConnectionRequest(dc.Context.Activity.CreateSenderConversationReference(), true);

            dc.State.SetValue(this.ResultProperty.GetValue(dc.State), messageRouterResult);

            await EmitEvent(dc, cancellationToken, "ConnectionRequest", messageRouterResult);

            switch (messageRouterResult.Type)
            {
                case ConnectionRequestResultType.Created:
                    await EmitEvent(dc, cancellationToken, "ConnectionRequestCreated", messageRouterResult);
                    break;

                case ConnectionRequestResultType.AlreadyExists:
                    await EmitEvent(dc, cancellationToken, "ConnectionRequestAlreadyExists", messageRouterResult);
                    break;

                case ConnectionRequestResultType.Rejected:
                    await EmitEvent(dc, cancellationToken, "ConnectionRequestCreatedRejected", messageRouterResult);
                    break;

                case ConnectionRequestResultType.NotSetup:
                    await EmitEvent(dc, cancellationToken, "ConnectionRequestCreatedNotSetup", messageRouterResult);
                    break;

                case ConnectionRequestResultType.Error:
                    await EmitEvent(dc, cancellationToken, "ConnectionRequestCreatedError", messageRouterResult);
                    break;
            }

            return await dc.EndDialogAsync(result: messageRouterResult, cancellationToken: cancellationToken);
        }

        private static async Task<bool> EmitEvent(DialogContext dc, CancellationToken cancellationToken, string eventName, object value)
        {
            bool handled;
            if (dc.Parent != null)
            {
                handled = await dc.Parent.EmitEventAsync(eventName, value, true, false, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                handled = await dc.EmitEventAsync(eventName, value, true, false, cancellationToken)
                    .ConfigureAwait(false);
            }

            return handled;
        }
    }
}
