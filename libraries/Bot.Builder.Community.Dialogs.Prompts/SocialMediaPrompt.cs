using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using static Microsoft.Recognizers.Text.Culture;

namespace Bot.Builder.Community.Dialogs.Prompts
{
    public enum SocialMediaPromptType
    {
        Mention,
        Hashtag
    }

    public class SocialMediaPrompt : Prompt<string>
    {
        public SocialMediaPrompt(string dialogId, SocialMediaPromptType type, PromptValidator<string> validator = null, string defaultLocale = null) 
            : base(dialogId, validator)
        {
            DefaultLocale = defaultLocale;
            PromptType = type;
        }

        public string DefaultLocale { get; set; }

        public SocialMediaPromptType PromptType { get; set; }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry, CancellationToken cancellationToken = new CancellationToken())
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

        protected override Task<PromptRecognizerResult<string>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = new CancellationToken())
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<string>();

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                var culture = turnContext.Activity.Locale ?? DefaultLocale ?? English;

                List<ModelResult> results = null;
                switch (PromptType)
                {
                    case SocialMediaPromptType.Hashtag:
                        results = Microsoft.Recognizers.Text.Sequence.SequenceRecognizer.RecognizeHashtag(message.Text, culture);
                        break;
                    case SocialMediaPromptType.Mention:
                        results = Microsoft.Recognizers.Text.Sequence.SequenceRecognizer.RecognizeMention(message.Text, culture);
                        break;
                }

                if (results?.Count > 0)
                {
                    result.Succeeded = true;
                    result.Value = results[0].Resolution["value"].ToString();
                }
            }

            return Task.FromResult(result);
        }
    }
}
