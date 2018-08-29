using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.Typing
{
    public class TypingMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.UserHasJustJoinedConversation() || turnContext.Activity.UserHasJustSentMessage())
            {
                await turnContext.Activity.DoWithTyping(async () =>
                {
                    await next(cancellationToken);
                });
            }
            else
            {
                await next(cancellationToken);
            }
        }
    }
}
