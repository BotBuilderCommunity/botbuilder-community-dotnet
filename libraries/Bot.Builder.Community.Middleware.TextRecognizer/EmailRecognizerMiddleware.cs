using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using static Microsoft.Recognizers.Text.Culture;
using Microsoft.Recognizers.Text.Sequence;

namespace Bot.Builder.Community.Middleware.TextRecognizer
{
    public class EmailRecognizerMiddleware : IMiddleware
    {
        private readonly string _defaultLocale;

        public EmailRecognizerMiddleware(string defaultLocale = null)
        {
            _defaultLocale = defaultLocale;
        }
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (turnContext.Activity.Type == ActivityTypes.Message &&
                !string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                var culture = turnContext.Activity.Locale ?? _defaultLocale ?? English;

                var recognizeEmail = SequenceRecognizer.RecognizeEmail(turnContext.Activity.Text, culture);
                if (recognizeEmail?.Count > 0)
                {
                    var value = recognizeEmail[0].Resolution["value"].ToString();
                    turnContext.TurnState.Add("EmailEntities",value);
                }
            }

            await next(cancellationToken);
        }
    }
}
