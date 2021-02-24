using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace Bot.Builder.Community.Components.CallDialogs
{
    /// <summary>
    /// Executes a list of dialogs in parallel. The dialogs SHOULD NOT prompt the user for input as 
    /// their dialog stacks will be thrown away at the end of the turn.
    /// </summary>
    public class CallDialogs : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "BotBuilderCommunity.CallDialogs";

        /// <summary>
        /// Initializes a new instance of the <see cref="CallDialogs"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public CallDialogs([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// Gets or sets the property path to store the array of dialog results in.
        /// </summary>
        /// <value>
        /// The property path to store the array of dialog results in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

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

            if (this.DialogsProperty == null)
            {
                throw new InvalidOperationException($"CallDialogs: operation couldn't be performed because the {nameof(DialogsProperty)} wasn't specified.");
            }

            // Get array of dialogs to call
            var property = this.DialogsProperty.GetValue(dc.State);
            var array = dc.State.GetValue<JArray>(property, () => new JArray());

            // Begin dialog calls
            var tasks = new List<Task<JToken>>();

            // Call dialog sharing this thread.
            var taskScheduler = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism: 1);
            //var taskScheduler = new CurrentThreadTaskScheduler();

            for (int iItem = 0; iItem < array.Count; iItem++)
            {
                var item = array[iItem];
                if (item is JObject pair && pair.ContainsKey("id"))
                {
                    var taskDc = new DialogContext(dc.Dialogs, dc, new DialogState()) { Parent = null };
                    var dialogId = pair["id"].ToString();
                    var dialogOptions = pair["options"];

                    tasks.Add(Task.Factory.StartNew(async () =>
                        {
                            Trace.WriteLine($"TaskBeginDialog {dialogId}({JsonConvert.SerializeObject(dialogOptions)})");
                            var result = await taskDc.BeginDialogAsync(dialogId, dialogOptions, cancellationToken);

                            // Ensure dialog completed
                            if (result.Status != DialogTurnStatus.Complete)
                            {
                                throw new InvalidOperationException($"CallDialogs: the dialog \"{dialogId}\" returned an invalid Status of \"{result.Status.ToString()}\".  Called dialogs should not wait for user input.");
                            }

                            Trace.WriteLine($"TaskBeginDialog {dialogId}({JsonConvert.SerializeObject(dialogOptions)}) RESULT => {JsonConvert.SerializeObject(result.Result ?? String.Empty)}");
                            // Return result value
                            return result.Result != null ? JToken.FromObject(result.Result) : null;
                        }, cancellationToken, TaskCreationOptions.AttachedToParent, taskScheduler).Unwrap());
                }
                else
                {
                    throw new InvalidOperationException($"CallDialogs: operation couldn't be performed because an invalid list of dialogs was passed in.");
                }
            }

            // Wait for calls to complete
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            // Copy results to output
            if (this.ResultProperty != null)
            {
                var resultProperty = this.ResultProperty.GetValue(dc.State);
                dc.State.SetValue(resultProperty, results);
            }

            return await dc.EndDialogAsync(results, cancellationToken);
        }

        public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            // when each dialog finishes it will call back as a continuation, but we are still in the BeginDialog waiting for them all to finish.
            throw new InvalidOperationException($"CallDialogs: operation couldn' because someone tried to resume to us. ");
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{DialogsProperty?.ToString()}]";
        }
    }
}
