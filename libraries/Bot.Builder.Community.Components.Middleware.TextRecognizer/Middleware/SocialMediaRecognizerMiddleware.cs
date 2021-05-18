using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Middleware
{
    public class SocialMediaRecognizerMiddleware : IMiddleware
    {
        private readonly ISocialMediaMiddlewareSettings _mediaMiddlewareSettings;
        public SocialMediaRecognizerMiddleware(ISocialMediaMiddlewareSettings mediaMiddlewareSettings)
        {
            _mediaMiddlewareSettings = mediaMiddlewareSettings ?? throw new ArgumentNullException(nameof(mediaMiddlewareSettings));
        }
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_mediaMiddlewareSettings.IsSocialMediaEnable && turnContext.Activity.Type == ActivityTypes.Message &&
                !string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                turnContext.Activity.Conversation.Properties.Remove(_mediaMiddlewareSettings.PropertyName);

                var culture = turnContext.Activity.Locale ?? _mediaMiddlewareSettings.Locale;

                List<ModelResult> modelResults = null;

                switch (_mediaMiddlewareSettings.MediaType)
                {
                    case SocialMediaType.Mention:
                        modelResults = SequenceRecognizer.RecognizeMention(turnContext.Activity.Text, culture);
                        break;
                    case SocialMediaType.Hashtag:
                        modelResults = SequenceRecognizer.RecognizeHashtag(turnContext.Activity.Text, culture);
                        break;
                }

                if (modelResults?.Count > 0)
                {
                    var value = modelResults[0].Resolution["value"].ToString();
                    turnContext.Activity.Conversation.Properties.Add(_mediaMiddlewareSettings.PropertyName,value);
                }
            }

            await next(cancellationToken);
        }
    }
}
