using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Middleware.TextRecognizer.Settings;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Bot.Builder.Community.Components.Middleware.TextRecognizer.Middleware
{
    public class InternetProtocolRecognizerMiddleware : IMiddleware
    {
        private readonly IInternetProtocolMiddlewareSettings _internetProtocolMiddlewareSettings;
        public InternetProtocolRecognizerMiddleware(IInternetProtocolMiddlewareSettings internetProtocolMiddlewareSettings)
        {
            _internetProtocolMiddlewareSettings = internetProtocolMiddlewareSettings ?? throw new ArgumentNullException(nameof(internetProtocolMiddlewareSettings));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_internetProtocolMiddlewareSettings.IsInternetProtocolEnable && turnContext.Activity.Type == ActivityTypes.Message &&
                !string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                var culture = turnContext.Activity.Locale ?? _internetProtocolMiddlewareSettings.Locale;

                List<ModelResult> modelResults = null;

                switch (_internetProtocolMiddlewareSettings.InternetProtocolType)
                {
                    case InternetProtocolType.IpAddress:
                        modelResults = SequenceRecognizer.RecognizeIpAddress(turnContext.Activity.Text, culture);
                        break;
                    case InternetProtocolType.Url:
                        modelResults = SequenceRecognizer.RecognizeURL(turnContext.Activity.Text, culture);
                        break;
                }

                if (modelResults?.Count > 0)
                {
                    var value = modelResults[0].Resolution["value"].ToString();

                    ObjectPath.SetPathValue(turnContext.TurnState, _internetProtocolMiddlewareSettings.Property, value);
                    
                }
            }

            await next(cancellationToken);
        }
    }
}
