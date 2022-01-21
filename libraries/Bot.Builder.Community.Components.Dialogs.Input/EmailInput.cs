using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Sequence;
using Newtonsoft.Json;
using static Microsoft.Recognizers.Text.Culture;

namespace Bot.Builder.Community.Components.Dialogs.Input
{
    public class EmailInput : InputDialog
    {
        [JsonProperty("$Kind")]
        public const string Kind = "BotBuilderCommunity.EmailInput";

        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }
        
        [JsonConstructor]
        public EmailInput([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        protected override Task<IActivity> OnRenderPromptAsync(DialogContext dc, InputState state,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (this.Prompt == null)
            {
                this.Prompt = new StaticActivityTemplate(MessageFactory.Text("Prompt for a email"));
            }

            return base.OnRenderPromptAsync(dc, state, cancellationToken);
        }

        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var validateText = dc.State.GetValue<object>(VALUE_PROPERTY);

            if (!(validateText is string strEmailText))
            {
                return Task.FromResult(InputState.Invalid);
            }

            var culture = GetCulture(dc);

            var recognizeEmail = SequenceRecognizer.RecognizeEmail(strEmailText, culture);

            if (recognizeEmail == null || recognizeEmail.Count <= 0)
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            var result = recognizeEmail[0].Resolution["value"].ToString();

            if (ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
            }

            return Task.FromResult(InputState.Valid);

        }

        private string GetCulture(DialogContext dc)
        {
            if (!string.IsNullOrEmpty(dc.Context.Activity.Locale))
            {
                return dc.Context.Activity.Locale;
            }

            return this.DefaultLocale != null ? this.DefaultLocale.GetValue(dc.State) : English;
        }
    }
}
