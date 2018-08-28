using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.SpellCheck
{
    public class SpellCheckMiddleware :IMiddleware
    {
        public SpellCheckMiddleware(IConfiguration configuration)
        {
            ApiKey = configuration.GetValue<string>("SpellCheckKey");
        }

        public string ApiKey { get; }
        
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            turnContext.Activity.Text = await turnContext.Activity.Text.SpellCheck(ApiKey);

            await next(cancellationToken);
        }
    }
}
