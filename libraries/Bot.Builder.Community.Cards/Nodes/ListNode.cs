using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Nodes
{
    internal class ListNode<T> : Node<IEnumerable<T>, T>
        where T : class
    {
        public ListNode(NodeType childNodeType, PayloadIdType? idType)
            : base(async (value, nextAsync) =>
            {
                foreach (var child in value)
                {
                    // The nextAsync return value is not needed here because
                    // the IEnumberable element references will remain unchanged
                    await nextAsync(child, childNodeType).ConfigureAwait(false);
                }

                return value;
            })
        {
            IdType = idType;
        }
    }
}
