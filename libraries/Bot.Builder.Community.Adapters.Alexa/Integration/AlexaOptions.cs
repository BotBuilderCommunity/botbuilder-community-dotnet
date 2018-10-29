// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Bot.Builder.Community.Adapters.Alexa.Integration
{
    public class AlexaOptions
    {
        public AlexaOptions()
        {
            ValidateIncomingAlexaRequests = true;
            ShouldEndSessionByDefault = false;
        }

        public bool ValidateIncomingAlexaRequests { get; set; }

        public bool ShouldEndSessionByDefault { get; set; }

        public bool TryConvertFirstActivityAttachmentToAlexaCard { get; set; }
    }
}
