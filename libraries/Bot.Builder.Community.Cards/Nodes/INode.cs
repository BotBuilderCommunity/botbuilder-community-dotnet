using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Nodes
{
    internal interface INode
    {
        IdType? IdType { get; set; }

        Task<object> CallChild(object value, Func<object, NodeType, Task<object>> nextAsync);

        Type GetTValue();
    }
}
