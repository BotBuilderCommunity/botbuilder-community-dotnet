using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Sequence;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Middleware
{
    public class EmailRecognizerMiddleware : IMiddleware
    {
        private readonly IEmailMiddlewareSettings _emailMiddlewareSettings;
        
        public EmailRecognizerMiddleware(IEmailMiddlewareSettings emailMiddlewareSettings)
        {
            _emailMiddlewareSettings = emailMiddlewareSettings ?? throw new ArgumentNullException(nameof(emailMiddlewareSettings));
        }
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_emailMiddlewareSettings.IsEmailEnable && turnContext.Activity.Type == ActivityTypes.Message && !string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                var culture = turnContext.Activity.Locale ?? _emailMiddlewareSettings.Locale;

                var recognizeEmail = SequenceRecognizer.RecognizeEmail(turnContext.Activity.Text, culture);
                if (recognizeEmail?.Count > 0)
                {
                    var value = recognizeEmail[0].Resolution["value"].ToString();

                    if (!string.IsNullOrEmpty(value))
                    {
                        ObjectPath.SetPathValue(turnContext.TurnState, _emailMiddlewareSettings.Property, value);
                    }
                }
            }
            await next(cancellationToken);
        }
    }
}
