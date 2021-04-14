using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.CallDialogs
{
    /// <summary>
    /// Appends a dialog to a list of dialogs that will be called in parallel using a CallDialogs action.
    /// </summary>
    public class AddDialogCall : Dialog, IDialogDependencies
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "BotBuilderCommunity.AddDialogCall";

        /// <summary>
        /// Initializes a new instance of the <see cref="AddDialogCall"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public AddDialogCall([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }


        /// <summary>
        /// Gets or sets property path expression to the list of dialogs to call.
        /// </summary>
        /// <value>
        /// Property path expression to the list of dialogs to call.
        /// </value>
        [JsonProperty("dialogsProperty")]
        public StringExpression DialogsProperty { get; set; }

        /// <summary>
        /// Gets or sets the dialog to call.
        /// </summary>
        /// <value>
        /// The dialog to call.
        /// </value>
        [JsonProperty("dialog")]
        public DialogExpression Dialog { get; set; }

        /// <summary>
        /// Gets or sets configurable options for the dialog. 
        /// </summary>
        /// <value>
        /// Configurable options for the dialog. 
        /// </value>
        [JsonProperty("options")]
        public ObjectExpression<object> Options { get; set; } = new ObjectExpression<object>();

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, Object options = null, CancellationToken cancellationToken = default)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (this.Dialog == null)
            {
                throw new InvalidOperationException($"AddDialogCall: operation couldn't be performed because the {nameof(Dialog)} wasn't specified.");
            }

            // Resolve dialog that was configured
            var dialog = this.ResolveDialog(dc);

            // use bindingOptions to bind to the bound options
            var boundOptions = BindOptions(dc, options);

            if (this.DialogsProperty == null)
            {
                throw new InvalidOperationException($"AddDialogCall: \"{dialog.Id}\" operation couldn't be performed because the {nameof(DialogsProperty)} wasn't specified.");
            }

            // Append dialog and options to array
            var property = this.DialogsProperty.GetValue(dc.State);
            var array = dc.State.GetValue<JArray>(property, () => new JArray());
            array.Add(new JObject()
            {
                {"id", dialog.Id },
                {"options", boundOptions }
            });
            dc.State.SetValue(property, array);

            return await dc.EndDialogAsync(null, cancellationToken);
        }

        /// <summary>
        /// Enumerates child dialog dependencies so they can be added to the containers dialog set.
        /// </summary>
        /// <returns>Dialog enumeration.</returns>
        public virtual IEnumerable<Dialog> GetDependencies()
        {
            if (Dialog?.Value != null)
            {
                yield return Dialog.Value;
            }

            yield break;
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{Dialog?.ToString()}]";
        }

        /// <summary>
        /// Resolve Dialog Expression as either Dialog, or StringExpression to get dialogid.
        /// </summary>
        /// <param name="dc">dialogcontext.</param>
        /// <returns>dialog.</returns>
        protected virtual Dialog ResolveDialog(DialogContext dc)
        {
            if (this.Dialog?.Value != null)
            {
                return this.Dialog.Value;
            }

            // NOTE: we want the result of the expression as a string so we can look up the string using external FindDialog().
            var se = new StringExpression($"={this.Dialog.ExpressionText}");
            var dialogId = se.GetValue(dc.State);
            return dc.FindDialog(dialogId ?? throw new InvalidOperationException($"{this.Dialog.ToString()} not found."));
        }

        /// <summary>
        /// BindOptions - evaluate expressions in options.
        /// </summary>
        /// <param name="dc">dialog context.</param>
        /// <param name="options">options to bind.</param>
        /// <returns>merged options with expressions bound to values.</returns>
        protected virtual JObject BindOptions(DialogContext dc, object options)
        {
            // binding options are static definition of options with overlay of passed in options);
            var bindingOptions = (JObject)ObjectPath.Merge(this.Options.GetValue(dc.State), options ?? new JObject());
            var boundOptions = new JObject();

            foreach (var binding in bindingOptions)
            {
                JToken value = null;

                // evaluate the value
                var (val, error) = new ValueExpression(binding.Value).TryGetValue(dc.State);

                if (error != null)
                {
                    throw new InvalidOperationException($"Unable to get a value for \"{binding.Value}\" from state. {error}");
                }

                if (val != null)
                {
                    value = JToken.FromObject(val).DeepClone();
                }

                value = value?.ReplaceJTokenRecursively(dc.State);

                // and store in options as the result
                ObjectPath.SetPathValue(boundOptions, binding.Key, value);
            }

            return boundOptions;
        }
    }
}
