using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Cards.Nodes
{
    internal class RichCardNode<T> : Node<T, IEnumerable<CardAction>>
        where T : class
    {
        public RichCardNode(Func<T, IEnumerable<CardAction>> buttonFactory)
            : base(async (card, nextAsync) =>
            {
                // The nextAsync return value is not needed here because the Buttons property reference will remain unchanged
                await nextAsync(buttonFactory(card), NodeType.CardActionList).ConfigureAwait(false);

                return card;
            })
        {
        }
    }
}
