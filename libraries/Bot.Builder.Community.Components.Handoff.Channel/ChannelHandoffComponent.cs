using AdaptiveExpressions.Converters;
using Bot.Builder.Community.Components.Handoff.Channel.Action;
using Bot.Builder.Community.Components.Handoff.Channel.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.Handoff.Channel
{
    public class ChannelHandoffComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>(new DeclarativeType<CreateHandoffRequest>(CreateHandoffRequest.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<AcceptHandoffRequest>(AcceptHandoffRequest.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<RejectHandoffRequest>(RejectHandoffRequest.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<GetCurrentHandoffRequests>(GetCurrentHandoffRequests.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<DisconnectHandoff>(DisconnectHandoff.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<RegisterForHandoffRequests>(RegisterForHandoffRequests.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<UnregisterForHandoffRequests>(UnregisterForHandoffRequests.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<NotifyAgentsNewRequest>(NotifyAgentsNewRequest.Kind));

            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<ConnectionRequest>>>();
        }
    }
}
