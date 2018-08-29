using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.SpellCheck
{
    public class SpellCheckMiddleware :IMiddleware
    {
        public SpellCheckMiddleware(IConfiguration configuration)
        {
            ApiKey = configuration.GetValue<string>("SpellCheckKey");
            CountryCode = configuration.GetValue<string>("SpellCheckCountryCode");
            Market = configuration.GetValue<string>("SpellCheckMarket");
        }

        public string ApiKey { get; }
        public string CountryCode { get; }
        public string Market { get; }

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            context.Activity.Text = await context.Activity.Text.SpellCheck(ApiKey, CountryCode, Market);

            await next();
        }
    }
}
