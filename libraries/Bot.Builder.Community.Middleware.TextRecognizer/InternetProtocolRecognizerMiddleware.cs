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
    public enum InternetProtocolPromptType
    {
        IpAddress,
        Url
    }

    public class InternetProtocolRecognizerMiddleware : IMiddleware
    {
        private readonly string _defaultLocale;
        private readonly InternetProtocolPromptType _internetProtocolPromptType;

        public InternetProtocolRecognizerMiddleware(InternetProtocolPromptType promptType, string defaultLocale = null)
        {
            _internetProtocolPromptType = promptType;
            _defaultLocale = defaultLocale;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (turnContext.Activity.Type == ActivityTypes.Message &&
                !string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                var culture = turnContext.Activity.Locale ?? _defaultLocale ?? English;
                
                List<ModelResult> modelResults = null;

                switch (_internetProtocolPromptType)
                {
                    case InternetProtocolPromptType.IpAddress:
                        modelResults = SequenceRecognizer.RecognizeIpAddress(turnContext.Activity.Text, culture);
                        break;
                    case InternetProtocolPromptType.Url:
                        modelResults = SequenceRecognizer.RecognizeURL(turnContext.Activity.Text, culture);
                        break;
                }

                if (modelResults?.Count > 0)
                {
                    var value = modelResults[0].Resolution["value"].ToString();
                    turnContext.TurnState.Add("InternetEntities", value);
                }
            }

            await next(cancellationToken);
        }
    }
}
