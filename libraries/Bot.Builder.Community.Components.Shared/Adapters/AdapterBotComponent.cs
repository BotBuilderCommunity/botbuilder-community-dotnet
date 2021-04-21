using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bot.Builder.Community.Components.Shared.Adapters
{
    /// <summary>
    /// Base adapter plugin for annotations-based options.
    /// </summary>
    /// <typeparam name="TAdapter">Adapter type.</typeparam>
    /// <typeparam name="TOptions">Adapter options type that defines the configuration required by the adapter.</typeparam>
    public class AdapterBotComponent<TAdapter, TOptions> : BotComponent
            where TAdapter : class, IBotFrameworkHttpAdapter
            where TOptions : class
    {
        /// <summary>
        /// Component entry point.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> where to register component services.</param>
        /// <param name="configuration"><see cref="IConfiguration"/> for the component. </param>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Get options from configuration
            var options = configuration.Get<TOptions>();

            // If the provided configuration is valid, register adapter and options.
            if (IsValidConfiguration(options))
            {
                services.AddSingleton(options);
                services.AddSingleton<IBotFrameworkHttpAdapter, TAdapter>();
            }
        }

        private static bool IsValidConfiguration(TOptions options)
        {
            if (options == null)
            {
                return false;
            }

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(options, serviceProvider: null, items: null);
            return Validator.TryValidateObject(options, validationContext, validationResults, validateAllProperties: true);
        }
    }
}
