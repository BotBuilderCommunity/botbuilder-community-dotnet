using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal class EnumerableTreeNode<T> : TreeNode<IEnumerable<T>, T>
        where T : class
    {
        public EnumerableTreeNode(ITreeNode childNode, string idType)
            : base(async (value, nextAsync) =>
            {
                foreach (var childValue in value)
                {
                    // The nextAsync return value is not needed here because
                    // the IEnumberable element references will remain unchanged
                    await nextAsync(childValue, childNode).ConfigureAwait(false);
                }

                return value;
            })
        {
            IdType = idType;
        }
    }
}
