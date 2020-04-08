using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal class RichCardTreeNode<T> : TreeNode<T, IEnumerable<CardAction>>
        where T : class
    {
        public RichCardTreeNode(Func<T, IEnumerable<CardAction>> buttonFactory)
            : base((card, next) =>
            {
                // The nextAsync return value is not needed here because the Buttons property reference will remain unchanged
                next(buttonFactory(card), TreeNodeType.CardActionList);

                return card;
            })
        {
        }
    }
}
