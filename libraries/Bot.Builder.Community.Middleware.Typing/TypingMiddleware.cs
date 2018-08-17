using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.Typing
{
    public class TypingMiddleware : IMiddleware
    {
        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Activity.UserHasJustJoinedConversation() || context.Activity.UserHasJustSentMessage())
            {
                await context.Activity.DoWithTyping(async () =>
                {
                    await next();
                });
            }
            else
            {
                await next();
            }
        }
    }
}
