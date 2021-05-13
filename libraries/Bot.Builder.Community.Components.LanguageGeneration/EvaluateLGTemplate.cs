using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.LanguageGeneration
{
    public class EvaluateLGTemplate : Dialog
    {
        [JsonConstructor]
        public EvaluateLGTemplate([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "BotBuilderCommunity.EvaluateLGTemplate";

        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        [JsonProperty("property")]
        public StringExpression Property { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

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

            // Get property name
            var property = Property?.GetValue(dc.State);
            if (String.IsNullOrEmpty(property))
            {
                throw new Exception($"{this.Id}: property name not specified.");
            }

            // Get LG template
            if (String.IsNullOrEmpty(Template))
            {
                throw new Exception($"{this.Id}: LG template not specified.");
            }

            // Evaluate template
            var languageGenerator = dc.Services.Get<LanguageGenerator>() ?? throw new MissingMemberException(nameof(LanguageGeneration));
            var lgResult = await languageGenerator.GenerateAsync(dc, Template, dc.State).ConfigureAwait(false);

            // Copy result to property
            dc.State.SetValue(property, lgResult);

            return await dc.EndDialogAsync(lgResult, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{Template?.ToString()}]";
        }
    }
}
