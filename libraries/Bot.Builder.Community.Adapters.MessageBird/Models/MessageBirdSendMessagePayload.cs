using MessageBird.Objects.Conversations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Adapters.MessageBird.Models
{
    public class MessageBirdSendMessagePayload
    {
        public string conversationId { get; set; }
        public ConversationMessageSendRequest conversationMessageRequest { get; set; }
    }
}
