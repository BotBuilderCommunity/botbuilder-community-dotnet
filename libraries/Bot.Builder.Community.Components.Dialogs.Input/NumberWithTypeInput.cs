using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using static Microsoft.Recognizers.Text.Culture;

namespace Bot.Builder.Community.Components.Dialogs.Input
{
    public class NumberWithTypeInput : InputDialog
    {
        [JsonProperty("$Kind")]
        public const string Kind = "BotBuilderCommunity.NumberWithTypeInput";

        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; }

        [JsonProperty("NumberType")]
        public EnumExpression<NumberWithTypeInputType> NumberType { get; set; } = NumberWithTypeInputType.Number;

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        [JsonConstructor]
        public NumberWithTypeInput([CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        protected override Task<IActivity> OnRenderPromptAsync(DialogContext dc, InputState state,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (this.Prompt == null)
            {
                this.Prompt = new StaticActivityTemplate(MessageFactory.Text("Prompt for a NumberWithTypeInput"));
            }

            return base.OnRenderPromptAsync(dc, state, cancellationToken);
        }
        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var validateText = dc.State.GetValue<object>(VALUE_PROPERTY);
            if (!(validateText is string))
            {
                return Task.FromResult(InputState.Invalid);
            }

            var culture = GetCulture(dc);
            List<ModelResult> results;

            var message = validateText.ToString();

            switch (NumberType.GetValue(dc.State))
            {
                case NumberWithTypeInputType.Percentage:
                    results = NumberRecognizer.RecognizePercentage(message, culture);
                    break;
                case NumberWithTypeInputType.Number:
                    results = NumberRecognizer.RecognizeNumber(message, culture);
                    break;
                case NumberWithTypeInputType.NumberRange:
                    results = NumberRecognizer.RecognizeNumberRange(message, culture);
                    break;
                case NumberWithTypeInputType.Ordinal:
                    results = NumberRecognizer.RecognizeOrdinal(message, culture);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (results == null || results.Count <= 0 || results[0].Resolution == null)
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            var result = new
            {
                Text = results[0].Text,
                Value = results[0].Resolution["value"].ToString(),
            };

            if (ResultProperty != null)
            {
                dc.State.SetValue(ResultProperty.GetValue(dc.State), result);
            }

            return Task.FromResult(InputState.Valid);
        }

        private string GetCulture(DialogContext dc)
        {
            if (!string.IsNullOrEmpty(dc.Context.Activity.Locale))
            {
                return dc.Context.Activity.Locale;
            }

            return DefaultLocale != null ? DefaultLocale.GetValue(dc.State) : English;
        }
    }

    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum NumberWithTypeInputType
    {
        Ordinal,
        Percentage,
        NumberRange,
        Number
    }
}