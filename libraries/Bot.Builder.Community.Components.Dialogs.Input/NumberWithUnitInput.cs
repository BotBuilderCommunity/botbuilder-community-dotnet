using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.NumberWithUnit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Microsoft.Recognizers.Text.Culture;

namespace Bot.Builder.Community.Components.Dialogs.Input
{
    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum NumberWithUnitInputType
    {
        Currency,
        Temperature,
        Age,
        Dimension
    }
    public class NumberWithUnitInput : InputDialog
    {
        [JsonProperty("$Kind")]
        public const string Kind = "BotBuilderCommunity.NumberWithUnitInput";

        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; }

        [JsonProperty("NumberUnit")]
        public EnumExpression<NumberWithUnitInputType> NumberUnit { get; set; } = NumberWithUnitInputType.Temperature;

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        [JsonConstructor]
        public NumberWithUnitInput([CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        protected override Task<IActivity> OnRenderPromptAsync(DialogContext dc, InputState state,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (Prompt == null)
            {
                Prompt = new StaticActivityTemplate(MessageFactory.Text("Prompt for a NumberWithUnitInput"));
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

            switch (NumberUnit.GetValue(dc.State))
            {
                case NumberWithUnitInputType.Temperature:
                    results = NumberWithUnitRecognizer.RecognizeTemperature(message, culture);
                    break;
                case NumberWithUnitInputType.Dimension:
                    results = NumberWithUnitRecognizer.RecognizeDimension(message, culture);
                    break;
                case NumberWithUnitInputType.Currency:
                    results = NumberWithUnitRecognizer.RecognizeCurrency(message, culture);
                    break;
                case NumberWithUnitInputType.Age:
                    results = NumberWithUnitRecognizer.RecognizeAge(message, culture);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (results == null || results.Count <= 0)
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            var resolvedUnit = results[0].Resolution["unit"].ToString();
            var resolvedValue = results[0].Resolution["value"].ToString();

            if (!double.TryParse(resolvedValue, out var value)) 
                return Task.FromResult(InputState.Valid);

            var result = new
            {
                Unit = resolvedUnit,
                Value = value,
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
}
