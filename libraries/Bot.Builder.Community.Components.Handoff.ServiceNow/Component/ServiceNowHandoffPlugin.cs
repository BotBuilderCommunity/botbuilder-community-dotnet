using Bot.Builder.Community.Components.Handoff.ServiceNow.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Bot.Builder.Community.Components.Handoff.ServiceNow.Component
{
    public class ServiceNowHandoffPlugin : IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            var services = context.Services;
            
            services.AddSingleton<IMiddleware, ServiceNowHandoffMiddleware>();
            services.AddTransient<ServiceNowHandoffController>();
            services.AddSingleton<ConversationHandoffRecordMap>();
            services.AddSingleton<IServiceNowCredentialsProvider>(sp => new ServiceNowCredentialsProvider(context.Configuration));
        }
    }
}
