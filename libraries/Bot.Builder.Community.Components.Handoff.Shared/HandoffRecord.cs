using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Handoff.Shared
{
    public abstract class HandoffRecord
    {
        protected HandoffRecord(ConversationReference conversationReference, string remoteConversationId)
        {
            ConversationReference = conversationReference;
            RemoteConversationId = remoteConversationId;
        }

        public ConversationReference ConversationReference { get; set; }

        public string RemoteConversationId { get; set; }
    }
}