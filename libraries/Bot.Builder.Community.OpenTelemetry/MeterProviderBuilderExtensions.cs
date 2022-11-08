using System;
using Bot.Builder.Community.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Internal;
using OpenTelemetry.Metrics;

namespace OpenTelemetry.Trace
{
    /// <summary>
    /// Extension methods to simplify registering of dependency instrumentation.
    /// </summary>
    public static class MeterProviderBuilderExtensions
    {
        /// <summary>
        /// Enables Bot Builder instrumentation.
        /// </summary>
        /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
        /// <param name="configureBotBuilderInstrumentationOptions">Bot Builder configuration options.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static MeterProviderBuilder AddBotBuilderInstrumentation(
            this MeterProviderBuilder builder)
        {
            
            builder.ConfigureServices(services => services.AddSingleton<BotOpenTelemetryClient>());

            builder.AddMeter(BotOpenTelemetryHelper.InstrumentationName);

            return builder;
        }
    }
}