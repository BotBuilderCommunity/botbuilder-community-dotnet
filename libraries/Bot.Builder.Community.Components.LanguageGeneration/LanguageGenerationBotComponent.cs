using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.LanguageGeneration
{
    /// <summary>
    /// <see cref="BotComponent"/> implementation for the storage actions.
    /// </summary>
    public class LanguageGenerationBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>(new DeclarativeType<EvaluateLGTemplate>(EvaluateLGTemplate.Kind));
        }
    }
}
