using System.Collections.Generic;

namespace Bot.Builder.Community.Cards
{
    public class BatchAndAttachmentsIds
    {
        internal BatchAndAttachmentsIds(string batchId, List<AttachmentsAndCardIds> attachmentsIds)
        {
            BatchId = batchId;
            AttachmentsIds = attachmentsIds;
        }

        public string BatchId { get; }

        public List<AttachmentsAndCardIds> AttachmentsIds { get; }
    }
}