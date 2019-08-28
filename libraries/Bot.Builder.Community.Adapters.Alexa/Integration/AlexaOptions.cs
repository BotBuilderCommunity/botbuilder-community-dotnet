// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Alexa.Integration
{
    public class AlexaOptions
    {
        public AlexaOptions()
        {
            ValidateIncomingAlexaRequests = true;
            ShouldEndSessionByDefault = false;
            DirectivesBackgroundImageByDefault = string.Empty;
            TittleTextByDefault = string.Empty;
        }

        public bool ValidateIncomingAlexaRequests { get; set; }

        public bool ShouldEndSessionByDefault { get; set; }

        public bool TryConvertFirstActivityAttachmentToAlexaCard { get; set; }

        public string DirectivesBackgroundImageByDefault { get; set; }

        public string TittleTextByDefault { get; set; }

        public Func<ITurnContext, Exception, Task> OnTurnError { get; set; }
    }
}
