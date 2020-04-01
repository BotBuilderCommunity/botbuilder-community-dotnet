using Bot.Builder.Community.Adapters.Google.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Bot.Builder.Community.Adapters.Google.Core.Model;

namespace Bot.Builder.Community.Adapters.Google
{
    public class GoogleAdapterOptions
    {
        public GoogleWebhookType WebhookType { get; set; } = GoogleWebhookType.Conversation;

        public bool ShouldEndSessionByDefault { get; set; } = true;

        public string ActionInvocationName { get; set; }

        public string ActionProjectId { get; set; }

        public bool ValidateIncomingRequests { get; set; }
    }
}
