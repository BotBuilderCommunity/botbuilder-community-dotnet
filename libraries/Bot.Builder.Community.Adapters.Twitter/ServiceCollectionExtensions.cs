using System;
using Bot.Builder.Community.Adapters.Twitter.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bot.Builder.Community.Adapters.Twitter
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTwitterAdapter(this IServiceCollection collection, Action<TwitterOptions> contextDelegate)
        {
            collection.AddSingleton<IHostedService, WebhookHostedService>();
            collection.AddSingleton<WebhookMiddleware>();
            collection.AddSingleton<TwitterAdapter>();

            collection.AddOptions();
            collection.Configure(contextDelegate);
        }
    }
}
