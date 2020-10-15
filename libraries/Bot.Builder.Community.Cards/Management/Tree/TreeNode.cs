using System;

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
        public TreeNode(ChildCallerDelegate<TValue, TChild> childCaller = null)
        {
            ChildCaller = childCaller;
        }

        public TreeNode(Func<TValue, Func<TChild, TreeNodeType, TChild>, TValue> childCaller)
        {
            ChildCaller = (value, next, reassignChildren) => childCaller(value, next);
        }

        public string IdScope { get; set; }

        private ChildCallerDelegate<TValue, TChild> ChildCaller { get; }

        public object CallChild(object value, Func<object, TreeNodeType, object> next, bool reassignChildren)
        {
            // This check will prevent child callers from needing to check for nulls
            if (value is TValue typedValue)
            {
                return ChildCaller?.Invoke(
                    typedValue,
                    (child, childType) =>
                    {
                        return next(child, childType) as TChild;
                    },
                    reassignChildren);
            }

            return value;
        }

        public Type GetTValue() => typeof(TValue);
    }
}
