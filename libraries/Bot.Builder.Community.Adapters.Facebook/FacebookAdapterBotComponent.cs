using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Adapters.Facebook
{
    /// <summary>
    /// <see cref="BotComponent"/> definition for <see cref="FacebookAdapter"/>.
    /// </summary>
    public class FacebookAdapterBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (FacebookAdapter.HasConfiguration(configuration))
            {
                // Components require the component configuration which is the subsection
                // assigned to the component. When the botbuilder-dotnet issue #5583 gets resolved, this could
                // change to the no-parameter overload.
                services.AddSingleton<IBotFrameworkHttpAdapter, FacebookAdapter>(sp => new FacebookAdapter(configuration));
            }
        }
    }
}
