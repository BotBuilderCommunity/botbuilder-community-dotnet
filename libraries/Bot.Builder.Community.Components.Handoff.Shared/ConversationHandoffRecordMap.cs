using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.Handoff.Shared
{
    public class ConversationHandoffRecordMap
    {
        private ConcurrentDictionary<string, HandoffRecord> _handoffRecords { get; set; } = new ConcurrentDictionary<string, HandoffRecord>();

        public virtual async Task<HandoffRecord> GetByRemoteConversationId(string conversationId)
        {
            return _handoffRecords.FirstOrDefault(h => h.Value.RemoteConversationId == conversationId).Value;
        }

        public virtual async Task<HandoffRecord> GetByConversationId(string conversationId)
        {
            return _handoffRecords.FirstOrDefault(h => h.Key == conversationId).Value;
        }

        public virtual async Task<bool> Add(string conversationId, HandoffRecord handoffRecord)
        {
            return _handoffRecords.TryAdd(conversationId, handoffRecord);
        }

        public virtual async Task<bool> Remove(string conversationId)
        {
            return _handoffRecords.TryRemove(conversationId, out HandoffRecord handoffRecord);
        }

        public bool TryUpdate(string convId, HandoffRecord newHandoffRecord, HandoffRecord existingHandoffRecord)
        {
            return _handoffRecords.TryUpdate(convId, newHandoffRecord, existingHandoffRecord);
        }
    }
}
