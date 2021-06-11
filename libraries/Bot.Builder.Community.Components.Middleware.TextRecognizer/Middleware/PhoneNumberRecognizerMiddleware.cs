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
    public class PhoneNumberRecognizerMiddleware : IMiddleware
    {
        private readonly IPhoneNumberMiddlewareSettings _phoneNumberMiddlewareSettings;
        
        public PhoneNumberRecognizerMiddleware(IPhoneNumberMiddlewareSettings phoneNumberMiddlewareSettings)
        {
            _phoneNumberMiddlewareSettings = phoneNumberMiddlewareSettings ?? throw new ArgumentNullException(nameof(phoneNumberMiddlewareSettings));
        }
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_phoneNumberMiddlewareSettings.IsPhoneNumberEnable && turnContext.Activity.Type == ActivityTypes.Message && !string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                var culture = turnContext.Activity.Locale ?? _phoneNumberMiddlewareSettings.Locale;

                var recognizePhoneNumber = SequenceRecognizer.RecognizePhoneNumber(turnContext.Activity.Text, culture);

                if (recognizePhoneNumber?.Count > 0)
                {
                    var value = recognizePhoneNumber[0].Resolution["value"].ToString();
                    
                    if (!string.IsNullOrEmpty(value))
                    {
                        ObjectPath.SetPathValue(turnContext.TurnState, _phoneNumberMiddlewareSettings.Property, value);
                    }
                }
            }

            await next(cancellationToken);
        }
    }
}
