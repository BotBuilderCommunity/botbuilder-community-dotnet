using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;
using static Microsoft.Recognizers.Text.Culture;

namespace Bot.Builder.Community.Middleware.TextRecognizer
{
    public enum SocialMediaPromptType
    {
        Mention,
        Hashtag
    }
    public class SocialMediaRecognizerMiddleware : IMiddleware
    {
        private readonly string _defaultLocale;

        private readonly SocialMediaPromptType _socialPromptType;
        public SocialMediaRecognizerMiddleware(SocialMediaPromptType promptType,string defaultLocale = null)
        {
            _defaultLocale = defaultLocale;
            _socialPromptType = promptType;
        }
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (turnContext.Activity.Type == ActivityTypes.Message &&
                !string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                var culture = turnContext.Activity.Locale ?? _defaultLocale ?? English;

                List<ModelResult> modelResults = null;

                switch (_socialPromptType)
                {
                    case SocialMediaPromptType.Mention:
                        modelResults = SequenceRecognizer.RecognizeMention(turnContext.Activity.Text, culture);
                        break;
                    case SocialMediaPromptType.Hashtag:
                        modelResults = SequenceRecognizer.RecognizeHashtag(turnContext.Activity.Text, culture);
                        break;
                }
                
                if (modelResults?.Count > 0)
                {
                    var value = modelResults[0].Resolution["value"].ToString();
                    turnContext.TurnState.Add("SocialEntities", value);
                }
            }

            await next(cancellationToken);
        }
    }
}
