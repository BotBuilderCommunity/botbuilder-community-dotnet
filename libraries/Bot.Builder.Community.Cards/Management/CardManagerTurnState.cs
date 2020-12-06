using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management
{
    internal class CardManagerTurnState
    {
        public IList<IActivity> MiddlewareIgnoreUpdate { get; } = new List<IActivity>();

        public IList<string> MiddlewareIgnoreDelete { get; } = new List<string>();
    }
}
