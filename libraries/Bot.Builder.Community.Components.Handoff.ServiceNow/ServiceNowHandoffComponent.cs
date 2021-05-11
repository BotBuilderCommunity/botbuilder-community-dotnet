using Bot.Builder.Community.Components.Handoff.ServiceNow;
using Bot.Builder.Community.Components.Handoff.ServiceNow.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.Handoff.LivePerson
{
    public class ServiceNowHandoffComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMiddleware, ServiceNowHandoffMiddleware>();
            services.AddTransient<ServiceNowHandoffController>();
            services.AddSingleton<ConversationHandoffRecordMap>();
            services.AddSingleton<IServiceNowCredentialsProvider>(sp => new ServiceNowCredentialsProvider(configuration));
        }
    }
}
