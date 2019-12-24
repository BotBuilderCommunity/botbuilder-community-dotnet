using System.Collections.Generic;

namespace Bot.Builder.Community.Cards
{
    public class CardAndActionIds
    {
        internal CardAndActionIds(string cardId, List<string> actionIds)
        {
            CardId = cardId;
            ActionIds = actionIds;
        }

        public string CardId { get; }

        public List<string> ActionIds { get; }
    }
}