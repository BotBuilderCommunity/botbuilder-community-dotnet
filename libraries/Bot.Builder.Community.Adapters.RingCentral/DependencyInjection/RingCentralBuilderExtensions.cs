using System;
using Bot.Builder.Community.Adapters.RingCentral;
using Bot.Builder.Community.Adapters.RingCentral.Middleware;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for configuring RingCentral using an <see cref="IRingCentralBuilder"/>.
    /// </summary>
    public static class IRingCentralBuilderExtensions
    {
        /// <summary>
        /// Registers a default downrendering middleware.
        /// </summary>
        /// <param name="builder">The <see cref="IRingCentralBuilder"/>RingCentralBuilder instance.</param>
        /// <returns>The <see cref="IRingCentralBuilder"/>Adds DownRenderingMiddleware singleton.</returns>
        public static IRingCentralBuilder AddDownrenderingMiddleware(
            this IRingCentralBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddSingleton<DownRenderingMiddleware>();

            return builder;
        }

        /// <summary>
        /// Registers a middleware that publishes bot conversation to a RingCentral custom source.
        /// </summary>
        /// <param name="builder">The <see cref="IRingCentralBuilder"/>RingCentralBuilder instance.</param>
        /// <returns>The <see cref="IRingCentralBuilder"/>Adds ActivityPublishingMiddleware singleton.</returns>
        public static IRingCentralBuilder AddConversationPublishMiddleware(
            this IRingCentralBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddSingleton<ActivityPublishingMiddleware>();

            return builder;
        }

        /// <summary>
        /// Registers an action to configure <see cref="RingCentralOptions"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRingCentralBuilder"/>RingCentralBuilder instance.</param>
        /// <param name="setupAction">An <see cref="Action{RingCentralOptions}"/>.</param>
        /// <returns>The <see cref="IRingCentralBuilder"/>.</returns>
        public static IRingCentralBuilder AddRingCentralOptions(
            this IRingCentralBuilder builder,
            Action<RingCentralOptions> setupAction)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            _ = setupAction ?? throw new ArgumentNullException(nameof(setupAction));

            builder.Services.Configure(setupAction);

            return builder;
        }
    }
}
