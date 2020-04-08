using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal delegate TValue ChildCallerDelegate<TValue, TChild>(TValue value, Func<TChild, TreeNodeType, TChild> nextAsync, bool reassignChildren);
}