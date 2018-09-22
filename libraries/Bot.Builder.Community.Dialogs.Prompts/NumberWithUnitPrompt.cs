using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.NumberWithUnit;
using static Microsoft.Recognizers.Text.Culture;

namespace Bot.Builder.Community.Dialogs.Prompts
{
    public class NumberWithUnitPrompt : Prompt<NumberWithUnitResult>
    {
        public NumberWithUnitPrompt(string dialogId, NumberWithUnitPromptType type, PromptValidator<NumberWithUnitResult> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            DefaultLocale = defaultLocale;
            PromptType = type;
        }

        public string DefaultLocale { get; set; }

        public NumberWithUnitPromptType PromptType { get; set; }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (isRetry && options.RetryPrompt != null)
            {
                await turnContext.SendActivityAsync(options.RetryPrompt, cancellationToken).ConfigureAwait(false);
            }
            else if (options.Prompt != null)
            {
                await turnContext.SendActivityAsync(options.Prompt, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override Task<PromptRecognizerResult<NumberWithUnitResult>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<NumberWithUnitResult>();

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                var culture = turnContext.Activity.Locale ?? DefaultLocale ?? English;

                List<Microsoft.Recognizers.Text.ModelResult> results = null;

                switch(PromptType)
                {
                    case NumberWithUnitPromptType.Currency:
                        results = NumberWithUnitRecognizer.RecognizeCurrency(message.Text, culture);
                        break;
                    case NumberWithUnitPromptType.Dimension:
                        results = NumberWithUnitRecognizer.RecognizeDimension(message.Text, culture);
                        break;
                    case NumberWithUnitPromptType.Age:
                        results = NumberWithUnitRecognizer.RecognizeAge(message.Text, culture);
                        break;
                    case NumberWithUnitPromptType.Temperature:
                        results = NumberWithUnitRecognizer.RecognizeTemperature(message.Text, culture);
                        break;
                }

                if (results?.Count > 0)
                {
                    var resolvedUnit = results[0].Resolution["unit"].ToString();
                    var resolvedValue = results[0].Resolution["value"].ToString();

                    if (double.TryParse(resolvedValue, out var value))
                    {
                        result.Succeeded = true;

                        var numberWithUnitResult = new NumberWithUnitResult
                        {
                            Unit = resolvedUnit,
                            Value = value
                        };

                        result.Value = numberWithUnitResult;
                    }
                }
            }

            return Task.FromResult(result);
        }
    }

    public class NumberWithUnitResult
    {
        public string Unit { get; set; }
        public dynamic Value { get; set; }
    }

    public enum NumberWithUnitPromptType
    {
        Currency,
        Temperature,
        Age,
        Dimension
    }
}
