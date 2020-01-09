using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Nodes
{
    internal delegate Task<TValue> ChildCallerDelegate<TValue, TChild>(TValue value, Func<TChild, NodeType, Task<TChild>> nextAsync);
}