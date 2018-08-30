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
    public class CurrencyPrompt : Prompt<CurrencyPromptResult>
    {
        public CurrencyPrompt(string dialogId, PromptValidator<CurrencyPromptResult> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            DefaultLocale = defaultLocale;
        }

        public string DefaultLocale { get; set; }

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

        protected override Task<PromptRecognizerResult<CurrencyPromptResult>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<CurrencyPromptResult>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                var culture = turnContext.Activity.Locale ?? DefaultLocale ?? English;
                var results = NumberWithUnitRecognizer.RecognizeCurrency(message.Text, culture);

                if (results.Count > 0)
                {
                    var resolvedUnit = results[0].Resolution["unit"].ToString();
                    var resolvedValue = results[0].Resolution["value"].ToString();

                    if (double.TryParse(resolvedValue, out var value))
                    {
                        result.Succeeded = true;

                        var promptResult = new CurrencyPromptResult
                        {
                            Unit = resolvedUnit,
                            Value = value
                        };

                        result.Value = (CurrencyPromptResult)(object)promptResult;
                    }
                }
            }

            return Task.FromResult(result);
        }
    }

    public class CurrencyPromptResult
    {
        public string Unit { get; set; }
        public double? Value { get; set; }
    }
}
