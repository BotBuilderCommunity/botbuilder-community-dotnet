// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bot.Builder.Community.Adapters.Google.Integration.AspNet.Core
{
    /// <summary>
    /// Extension class for bot integration with ASP.NET Core 2.0 projects.
    /// </summary>
    /// <seealso cref="GoogleBotPaths"/>
    /// <seealso cref="GoogleAdapter"/>
    /// <seealso cref="ServiceCollectionExtensions"/>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Initializes and adds a bot adapter to the HTTP request pipeline, using default endpoint paths for the bot.
        /// </summary>
        /// <param name="applicationBuilder">The application builder for the ASP.NET application.</param>
        /// <returns>The updated application builder.</returns>
        /// <remarks>This method adds any middleware from the <see cref="GoogleBotOptions"/> provided in the
        /// <see cref="ServiceCollectionExtensions.AddgoogleBot{TBot}(IServiceCollection, Action{GoogleBotOptions})"/>
        /// method to the adapter.</remarks>
        public static IApplicationBuilder Usegoogle(this IApplicationBuilder applicationBuilder) =>
            applicationBuilder.Usegoogle(paths => {});

        /// <summary>
        /// Initializes and adds a bot adapter to the HTTP request pipeline, using custom endpoint paths for the bot.
        /// </summary>
        /// <param name="applicationBuilder">The application builder for the ASP.NET application.</param>
        /// <param name="configurePaths">Allows you to modify the endpoints for the bot.</param>
        /// <returns>The updated application builder.</returns>
        /// <remarks>This method adds any middleware from the <see cref="GoogleBotOptions"/> provided in the
        /// <see cref="ServiceCollectionExtensions.AddBot{TBot}(IServiceCollection, Action{GoogleBotOptions})"/>
        /// method to the adapter.</remarks>
        public static IApplicationBuilder Usegoogle(this IApplicationBuilder applicationBuilder, Action<GoogleBotPaths> configurePaths)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            if (configurePaths == null)
            {
                throw new ArgumentNullException(nameof(configurePaths));
            }

            var options = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<GoogleBotOptions>>().Value;

            var googleAdapter = new GoogleAdapter();

            foreach (var middleware in options.Middleware)
            {
                googleAdapter.Use(middleware);
            }

            var paths = options.Paths;

            configurePaths(paths);

            if (!options.Paths.BasePath.EndsWith("/"))
            {
                options.Paths.BasePath += "/";
            }

            applicationBuilder.Map(
                $"{paths.BasePath}{paths.SkillRequestsPath}", 
                botActivitiesAppBuilder => botActivitiesAppBuilder.Run(new GoogleRequestHandler(googleAdapter, options.googleOptions).HandleAsync));

            return applicationBuilder;
        }
    }
}
