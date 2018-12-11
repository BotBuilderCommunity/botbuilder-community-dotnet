// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Adapters.Google.Integration.AspNet.Core
{
    public interface IBotConfigurationBuilder
    {
        List<IMiddleware> Middleware { get; }
    }
}
