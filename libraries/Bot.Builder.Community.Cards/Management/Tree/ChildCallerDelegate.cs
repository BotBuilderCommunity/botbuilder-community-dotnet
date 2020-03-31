using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal delegate Task<TValue> ChildCallerDelegate<TValue, TChild>(TValue value, Func<TChild, ITreeNode, Task<TChild>> nextAsync);
}