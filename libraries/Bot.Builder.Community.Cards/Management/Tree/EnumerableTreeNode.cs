using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal class EnumerableTreeNode<T> : TreeNode<IEnumerable<T>, T>
        where T : class
    {
        public EnumerableTreeNode(TreeNodeType childNodeType, string idType)
            : base((value, next) =>
            {
                foreach (var child in value)
                {
                    // The nextAsync return value is not needed here because
                    // the IEnumberable element references will remain unchanged
                    next(child, childNodeType);
                }

                return value;
            })
        {
            IdType = idType;
        }
    }
}
