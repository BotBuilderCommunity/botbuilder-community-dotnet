using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Dialogs.ChoiceFlow.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SelectRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getChildren)
        {
            if (null == source) throw new ArgumentNullException("source");
            if (null == getChildren) return source;
            return SelectRecursiveIterator(source, getChildren);
        }


        private static IEnumerable<T> SelectRecursiveIterator<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> getChildren)
        {
            var stack = new Stack<IEnumerator<T>>();

            try
            {
                stack.Push(source.GetEnumerator());
                while (0 != stack.Count)
                {
                    var iter = stack.Peek();
                    if (iter.MoveNext())
                    {
                        T current = iter.Current;
                        yield return current;

                        var children = getChildren(current);
                        if (null != children) stack.Push(children.GetEnumerator());
                    }
                    else
                    {
                        iter.Dispose();
                        stack.Pop();
                    }
                }
            }
            finally
            {
                while (0 != stack.Count)
                {
                    stack.Pop().Dispose();
                }
            }
        }
    }
}
