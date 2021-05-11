using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Storage
{
    public class DeleteStorageItem : Dialog
    {
        [JsonConstructor]
        public DeleteStorageItem([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "BotBuilderCommunity.DeleteStorageItem";

        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        [JsonProperty("itemKey")]
        public StringExpression ItemKey { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // Get current storage provider
            var storage = dc.Context.TurnState.Get<IStorage>();
            if (storage == null)
            {
                throw new Exception($"{this.Id}: storage provider could not be found in turn state.");
            }

            // Delete item
            var itemKey = this.ItemKey.GetValue(dc.State);
            if (!String.IsNullOrEmpty(itemKey))
            {
                await storage.DeleteAsync(new string[] { itemKey }, cancellationToken).ConfigureAwait(false);
            }

            return await dc.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
        }
    }
}
