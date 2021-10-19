using Bot.Builder.Community.Components.Handoff.LivePerson.Models;
using Bot.Builder.Community.Components.Handoff.Shared;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Handoff.LivePerson
{
    public class LivePersonHandoffRecord : HandoffRecord
    {
        public LivePersonHandoffRecord(ConversationReference conversationReference, LivePersonConversationRecord conversationRecord) 
            : base(conversationReference, conversationRecord.ConversationId)
        {
            ConversationRecord = conversationRecord;
        }

        public LivePersonConversationRecord ConversationRecord { get; set; }
    }
}
