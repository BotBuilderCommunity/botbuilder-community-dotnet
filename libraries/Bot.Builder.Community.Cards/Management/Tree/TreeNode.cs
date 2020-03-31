using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal class TreeNode<TValue, TChild> : ITreeNode
        where TValue : class
        where TChild : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode{TValue, TChild}"/> class.
        /// </summary>
        /// <param name="childCaller">A delegate that gets passed a value that's guaranteed to not be null.</param>
        public TreeNode(ChildCallerDelegate<TValue, TChild> childCaller)
        {
            ChildCaller = childCaller;
        }

        public string IdType { get; set; }

        private ChildCallerDelegate<TValue, TChild> ChildCaller { get; }

        public async Task<object> CallChildAsync(object value, Func<object, ITreeNode, Task<object>> nextAsync)
        {
            // This check will prevent child callers from needing to check for nulls
            if (value is TValue typedValue)
            {
                return await ChildCaller(
                    typedValue,
                    async (childValue, childNode) =>
                    {
                        return await nextAsync(childValue, childNode).ConfigureAwait(false) as TChild;
                    }).ConfigureAwait(false);
            }

            return value;
        }

        public Type GetTValue() => typeof(TValue);
    }
}
