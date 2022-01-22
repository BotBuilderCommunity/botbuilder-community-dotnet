using System;
using Bot.Builder.Community.Components.Trigger.ExpireConversation.Middleware;
using Bot.Builder.Community.Components.Trigger.ExpireConversation.Trigger;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.Trigger.ExpireConversation
{
    public class ExpireConversationMiddlewareComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if(services == null)
                throw new ArgumentNullException(nameof(services));  

            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            services.AddSingleton<IMiddleware, ExpireConversationMiddleware>();
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnExpireConversation>(OnExpireConversation.Kind));
        }
    }
}
