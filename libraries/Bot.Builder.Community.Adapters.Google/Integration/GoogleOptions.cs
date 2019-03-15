// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Google.Integration
{
    public class GoogleOptions
    {
        public GoogleOptions()
        {
            ValidateIncominggoogleRequests = true;
            ShouldEndSessionByDefault = false;
        }

        public bool ValidateIncominggoogleRequests { get; set; }

        public bool ShouldEndSessionByDefault { get; set; }

        public bool TryConvertFirstActivityAttachmentTogoogleCard { get; set; }

        public Func<ITurnContext, Exception, Task> OnTurnError { get; set; }

        public string ActionInvocationName { get; set; }
    }
}
