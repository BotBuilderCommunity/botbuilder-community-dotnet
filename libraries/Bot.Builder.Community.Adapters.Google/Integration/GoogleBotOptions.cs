// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Bot.Builder.Community.Adapters.Google.Integration.AspNet.Core;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Adapters.Google.Integration
{
    /// <summary>
    /// Contains settings that your ASP.NET application uses to initialize the <see cref="BotAdapter"/>
    /// that it adds to the HTTP request pipeline.
    /// </summary>
    /// <seealso cref="ApplicationBuilderExtensions"/>
    public class GoogleBotOptions
    {
        private readonly List<IMiddleware> _middleware;
        private readonly GoogleBotPaths _paths;

        /// <summary>
        /// Creates a <see cref="GoogleBotOptions"/> object.
        /// </summary>
        public GoogleBotOptions()
        {
            _middleware = new List<IMiddleware>();
            _paths = new GoogleBotPaths();

            googleOptions = new GoogleOptions();
        }

        /// <summary>
        /// The middleware collection with which to initialize the adapter.
        /// </summary>
        public IList<IMiddleware> Middleware { get => _middleware; }

        public GoogleBotPaths Paths { get => _paths; }

        public GoogleOptions googleOptions { get; set; }
    }
}
