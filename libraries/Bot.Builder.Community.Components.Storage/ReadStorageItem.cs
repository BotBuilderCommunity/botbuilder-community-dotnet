using System;
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
    public class ReadStorageItem : Dialog
    {
        [JsonConstructor]
        public ReadStorageItem([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "BotBuilderCommunity.ReadStorageItem";

        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        [JsonProperty("itemKey")]
        public StringExpression ItemKey { get; set; }

        [JsonProperty("initialItem")]
        public ValueExpression InitialItem { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

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

            var storage = dc.Context.TurnState.Get<IStorage>();
            if (storage == null)
            {
                throw new Exception($"{this.Id}: storage provider could not be found in turn state.");
            }

            // Read item and convert to JObject
            var itemKey = this.ItemKey.GetValue(dc.State);
            var items = await storage.ReadAsync(new string[] { itemKey }, cancellationToken).ConfigureAwait(false);
            var item = items.ContainsKey(itemKey) ? items[itemKey] : null;
            var jItem = item != null ? JToken.FromObject(item) : null;

            // Assign initial object if null
            if (jItem == null && this.InitialItem != null)
            {
                var initialItem = this.InitialItem.GetValue(dc.State);
                if (initialItem is JToken jInitialItem)
                {
                    jItem = jInitialItem;
                }
            }

            // Save item
            if (this.ResultProperty != null)
            {
                var resultsProp = this.ResultProperty.GetValue(dc.State);
                dc.State.SetValue(resultsProp, jItem);
            }

            return await dc.EndDialogAsync(jItem, cancellationToken).ConfigureAwait(false);
        }
    }
}
