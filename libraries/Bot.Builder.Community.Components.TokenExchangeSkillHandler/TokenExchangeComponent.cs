using System;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.TokenExchangeSkillHandler
{
    /// <summary>
    /// <see cref="BotComponent"/> for registering the TokenExchangeHandler with the ServicesCollection.
    /// </summary>
    public class TokenExchangeComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var settings = configuration.Get<ComponentSettings>() ?? new ComponentSettings();
            if (settings.UseTokenExchangeSkillHandler)
            {
                services.AddSingleton<ChannelServiceHandlerBase, TokenExchangeSkillHandler>();
            }
        }
    }
}
