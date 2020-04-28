using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management
{
    internal class CardManagerTurnState
    {
        public IList<string> MiddlewareIgnoreUpdate { get; } = new List<string>();

        public IList<string> MiddlewareIgnoreDelete { get; } = new List<string>();
    }
}
