using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal class ListTreeNode<T> : TreeNode<IEnumerable<T>, T>
        where T : class
    {
        public ListTreeNode(TreeNodeType childNodeType, string idType)
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
