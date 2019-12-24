using System.Collections.Generic;

namespace Bot.Builder.Community.Cards
{
    public class AttachmentsAndCardIds
    {
        internal AttachmentsAndCardIds(string attachmentsId, List<CardAndActionIds> cardIds)
        {
            AttachmentsId = attachmentsId;
            CardIds = cardIds;
        }

        public string AttachmentsId { get; }

        public List<CardAndActionIds> CardIds { get; }
    }
}