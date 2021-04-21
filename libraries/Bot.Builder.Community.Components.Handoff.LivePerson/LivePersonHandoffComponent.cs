using Bot.Builder.Community.Components.Handoff.LivePerson.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.Handoff.LivePerson
{
    public class LivePersonHandoffComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMiddleware, LivePersonHandoffMiddleware>();
            services.AddTransient<LivePersonHandoffController>();
            services.AddSingleton<ConversationHandoffRecordMap>();
            services.AddSingleton<ILivePersonCredentialsProvider>(sp => new LivePersonCredentialsProvider(configuration));
        }
    }
}
