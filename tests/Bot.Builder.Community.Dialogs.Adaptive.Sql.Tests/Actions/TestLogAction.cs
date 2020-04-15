using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Basic assertion TestAction, which validates assertions against a reply activity.
    /// </summary>
    [DebuggerDisplay("AssertReplyActivity:{GetConditionDescription()}")]
    public class TestLogAction : TestAction
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.Test.TestLogAction";

        [JsonConstructor]
        public TestLogAction([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the description of this assertion.
        /// </summary>
        /// <value>Description of what this assertion is.</value>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the milliseconds to wait for a reply.
        /// </summary>
        /// <value>the milliseceods to wait.</value>
        [DefaultValue(3000)]
        [JsonProperty("timeout")]
        public uint Timeout { get; set; } = 3000;

        /// <summary>
        /// Gets or sets the assertions.
        /// </summary>
        /// <value>The expressions for assertions.</value>
        [JsonProperty("text")]
        public string Text { get; set; }

        public virtual string GetConditionDescription()
        {
            return Description ?? Text;
        }

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            var timeout = (int)this.Timeout;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                timeout = int.MaxValue;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter((int)timeout);
            IActivity replyActivity = await adapter.GetNextReplyAsync(cts.Token).ConfigureAwait(false);

            if (replyActivity != null)
            {

                var (result, error) = new ObjectExpression<object>(Text).TryGetValue(replyActivity);
                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception($"{this.Description} {Text} {replyActivity}");
                }

                System.Diagnostics.Trace.TraceInformation(JsonConvert.SerializeObject(result, Formatting.Indented));
                return;
            }
        }
    }
}