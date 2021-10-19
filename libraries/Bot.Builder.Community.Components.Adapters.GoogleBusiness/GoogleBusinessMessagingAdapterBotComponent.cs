using AdaptiveExpressions.Converters;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Action;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Action.Model;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model;
using Bot.Builder.Community.Components.Shared.Adapters;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness
{
    /// <summary>
    /// Google Business Messaging adapter bot plugin.
    /// </summary>
    public class GoogleBusinessMessagingAdapterBotComponent : AdapterBotComponent<GoogleBusinessMessagingAdapter, GoogleBusinessMessagingAdapterOptions>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            base.ConfigureServices(services, configuration);

            services.AddSingleton<DeclarativeType>(new DeclarativeType<SendGBMActivity>(SendGBMActivity.Kind));
            services.AddSingleton<DeclarativeType>(new DeclarativeType<SendGBMSurvey>(SendGBMSurvey.Kind));
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<OpenUrlActionProperty>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<AuthenticationRequestSuggestion>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<LiveAgentRequestSuggestion>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<DialActionSuggestion>>>();
        }
    }
}
