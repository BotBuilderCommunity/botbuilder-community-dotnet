using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Recognizers.Text.Culture;

namespace Bot.Builder.Community.Dialogs.Prompts
{
    public enum NumberWithTypePromptType
    {
        Ordinal,
        Percentage,
        NumberRange,
        Number
    }

    public class NumberWithTypePrompt : Prompt<NumberWithTypeResult>
    {
        public NumberWithTypePrompt(string dialogId, NumberWithTypePromptType type,PromptValidator<NumberWithTypeResult> validator=null, string defaultLocale = null)
        : base(dialogId, validator)
        {
            DefaultLocale = defaultLocale;
            PromptType = type;
        }

        public string DefaultLocale { get; set; }

        public NumberWithTypePromptType PromptType { get; set; }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry,CancellationToken cancellationToken = new CancellationToken())
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

        protected override Task<PromptRecognizerResult<NumberWithTypeResult>> OnRecognizeAsync(ITurnContext turnContext,IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = new CancellationToken())
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<NumberWithTypeResult>();

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                var culture = turnContext.Activity.Locale ?? DefaultLocale ?? English;

                List<ModelResult> results = null;
                switch (PromptType)
                {
                    case NumberWithTypePromptType.Ordinal:
                        results = NumberRecognizer.RecognizeOrdinal(message.Text, culture);
                        break;
                    case NumberWithTypePromptType.Percentage:
                        results = NumberRecognizer.RecognizePercentage(message.Text, culture);
                        break;
                    case NumberWithTypePromptType.NumberRange:
                        results = NumberRecognizer.RecognizeNumberRange(message.Text, culture);
                        break;
                    case NumberWithTypePromptType.Number:
                        results = NumberRecognizer.RecognizeNumber(message.Text, culture);
                        break;
                }

                if (results?.Count > 0)
                {
                    var resolution = results[0].Resolution;
                    if (resolution != null)
                    {
                        result.Succeeded = true;
                        var numberWithTypeResult = new NumberWithTypeResult()
                        {
                            Text = results[0].Text,
                            Value = resolution["value"].ToString()
                        };

                        result.Value = numberWithTypeResult;
                    }
                }
            }

            return Task.FromResult(result);
        }
    }

    public class NumberWithTypeResult
    {
        public string Text { get; set; }

        public dynamic Value { get; set; }
    }
}
