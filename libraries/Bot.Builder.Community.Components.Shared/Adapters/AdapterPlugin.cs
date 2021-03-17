using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Plugins;
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
    public class AdapterPlugin<TAdapter, TOptions> : IBotPlugin
            where TAdapter : class, IBotFrameworkHttpAdapter
            where TOptions : class
    {
        /// <summary>
        /// Plugin entry point.
        /// </summary>
        /// <param name="context">Plugin load context.</param>
        public void Load(IBotPluginLoadContext context)
        {
            IServiceCollection services = context.Services;
            IConfiguration configuration = context.Configuration;

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
