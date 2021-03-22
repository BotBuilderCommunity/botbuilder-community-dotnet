using Bot.Builder.Community.Components.Handoff.LivePerson.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Bot.Builder.Community.Components.Handoff.LivePerson.Component
{
    public class LivePersonHandoffPlugin : IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            var services = context.Services;
            
            services.AddSingleton<IMiddleware, LivePersonHandoffMiddleware>();
            services.AddTransient<LivePersonHandoffController>();
            services.AddSingleton<ConversationHandoffRecordMap>();
            services.AddSingleton<ILivePersonCredentialsProvider>(sp => new LivePersonCredentialsProvider(context.Configuration));
        }
    }
}
