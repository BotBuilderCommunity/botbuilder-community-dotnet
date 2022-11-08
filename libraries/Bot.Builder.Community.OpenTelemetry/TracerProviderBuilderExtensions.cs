using System;
using Bot.Builder.Community.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Trace
{
    /// <summary>
    /// Extension methods to simplify registering of dependency instrumentation.
    /// </summary>
    public static class TracerProviderBuilderExtensions
    {
        /// <summary>
        /// Enables Bot Builder instrumentation.
        /// </summary>
        /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
        /// <param name="configureBotBuilderInstrumentationOptions">Bot Builder configuration options.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static TracerProviderBuilder AddBotBuilderInstrumentation(
            this TracerProviderBuilder builder)
        {
            // Guard.ThrowIfNull(builder);

            builder.ConfigureServices(services => services.AddSingleton<BotOpenTelemetryClient>());

            builder.AddSource(BotOpenTelemetryHelper.InstrumentationName);

            return builder;
        }
    }
}