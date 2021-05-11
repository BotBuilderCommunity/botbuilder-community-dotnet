using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Components.Storage
{
    public class WriteStorageItem : Dialog
    {
        [JsonConstructor]
        public WriteStorageItem([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "BotBuilderCommunity.WriteStorageItem";

        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        [JsonProperty("item")]
        public ValueExpression Item { get; set; }

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

            // Get item from memory
            var item = this.Item.GetValue(dc.State);
            if (!(item is JObject))
            {
                throw new Exception($"{this.Id}: \"item\" is null or not an object.");
            }

            // Get item key from memory
            var itemKey = this.ItemKey.GetValue(dc.State);
            if (String.IsNullOrEmpty(itemKey))
            {
                throw new Exception($"{this.Id}: \"itemKey\" is null or an empty string.");
            }

            // Create change list
            var changes = new Dictionary<string, object>();
            changes.Add(itemKey, (item as JObject).ToObject<IDictionary<string, object>>());

            // Write changes
            await storage.WriteAsync(changes, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
        }
    }
}
