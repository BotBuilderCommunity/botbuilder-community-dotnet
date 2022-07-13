using System;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Announcer;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Helper;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Middleware;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Service;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Trigger;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent
{
    public class SessionAgentBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            ActivityHelper.IsEnable = string.IsNullOrEmpty(configuration["IsEnabled"]) 
                                      || Convert.ToBoolean(configuration["IsEnabled"]);

            if (!ActivityHelper.IsEnable) return;

            ActivityHelper.SleepInMilliseconds = string.IsNullOrEmpty(configuration["SleepTime"]) ? 1000 : Convert.ToInt32(configuration["SleepTime"]);

            if (ActivityHelper.SleepInMilliseconds <= 100)
                ActivityHelper.SleepInMilliseconds = 1000;

            services.AddSingleton<IServiceAgentAnnouncer,ServiceAgentAnnouncer>();

            services.AddHostedService<AgentBackgroundService>();

            services.AddSingleton<IMiddleware, ConversationAgentMiddleware>();

            services.AddSingleton<DeclarativeType>(sp =>
                new DeclarativeType<OnSessionExpireConversation>(OnSessionExpireConversation.Kind));

            services.AddSingleton<DeclarativeType>(sp =>
                new DeclarativeType<OnReminderConversation>(OnReminderConversation.Kind));
        }
    }
}