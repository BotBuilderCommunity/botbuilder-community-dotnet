using System;
using Bot.Builder.Community.Adapters.Twitter;
using Bot.Builder.Community.Adapters.Twitter.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseTwitterAdapter(this IApplicationBuilder app)
        {
            var twitterOptions = app.ApplicationServices.GetRequiredService<IOptions<TwitterOptions>>().Value;
            var uriPath = new Uri(twitterOptions.WebhookUri);

            app.UseWhen(
                context => context.Request.Path.StartsWithSegments(uriPath.AbsolutePath), 
                builder => builder.UseMiddleware<WebhookMiddleware>());
        }
    }
}