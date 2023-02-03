using System;
using Bot.Builder.Community.Components.Middleware.Multilingual.AzureTranslateService;
using Bot.Builder.Community.Components.Middleware.Multilingual.Middleware;
using Bot.Builder.Community.Components.Middleware.Multilingual.Setting;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Builder.Community.Components.Middleware.Multilingual
{
    public class MultilingualMiddlewareComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {

        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        services.AddSingleton<ISetting>(sp => new Settings(configuration));

        services.AddSingleton<ITranslateService, TranslateService>();

        services.AddSingleton<IMiddleware, MultilingualMiddleware>();

    }
}
}
