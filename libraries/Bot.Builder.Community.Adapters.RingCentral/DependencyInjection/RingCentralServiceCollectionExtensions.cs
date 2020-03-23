using System;
using Bot.Builder.Community.Adapters.RingCentral;
using Bot.Builder.Community.Adapters.RingCentral.Handoff;
using Bot.Builder.Community.Adapters.RingCentral.Renderer;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding RingCentral integration support to the DI container.
    /// </summary>
    public static class RingCentralServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required for using RingCentral bot integration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>An <see cref="IRingCentralBuilder"/> that can be used to further configure the RingCentral services.</returns>
        public static IRingCentralBuilder AddRingCentral(this IServiceCollection services)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            var builder = new RingCentralBuilder(services);

            builder
                // CONSIDER removing this extension and add them directly to services.
                .AddDownrenderingMiddleware() 
                .AddConversationPublishMiddleware();

            services.TryAddSingleton<RingCentralClientWrapper>();
            services.TryAddSingleton<IHandoffRequestRecognizer, StaticHandoffRequestRecognizer>();

            // Register WhatsAppRenderer
            services.TryAddSingleton<IWhatsAppRenderer, WhatsAppRenderer>();

            return builder;
        }

        /// <summary>
        /// Adds services required for using RingCentral bot integration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{RingCentralOptions}"/> to configure the provided <see cref="RingCentralOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddRingCentral(this IServiceCollection services, Action<RingCentralOptions> setupAction)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = setupAction ?? throw new ArgumentNullException(nameof(setupAction));

            services.AddRingCentral();

            services.Configure(setupAction);

            return services;
        }
    }
}
