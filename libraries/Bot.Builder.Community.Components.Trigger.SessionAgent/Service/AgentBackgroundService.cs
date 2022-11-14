using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Announcer;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Service
{
    public class AgentBackgroundService : BackgroundService
    {
        private readonly ILogger<AgentBackgroundService> _logger;

        private readonly IServiceAgentAnnouncer _serviceAgentAnnouncer = null;
        public AgentBackgroundService(IBot bot, IBotFrameworkHttpAdapter botHttpAdapter, ILogger<AgentBackgroundService> logger,
            IServiceAgentAnnouncer serviceAgentAnnouncer)
        {
            _logger = logger;
            _serviceAgentAnnouncer = serviceAgentAnnouncer;
            _serviceAgentAnnouncer.SetController(bot,botHttpAdapter);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _serviceAgentAnnouncer?.SendAnnouncement();
                await Task.Delay(TimeSpan.FromMilliseconds(ActivityHelper.SleepInMilliseconds), stoppingToken);
            }
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Tracker started : {dataTime}", DateTime.UtcNow);
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Tracker stopped : {dataTime}", DateTime.UtcNow);
            return base.StopAsync(cancellationToken);
        }
    }
}