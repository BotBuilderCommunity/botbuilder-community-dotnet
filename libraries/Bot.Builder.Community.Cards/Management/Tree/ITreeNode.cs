using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal interface ITreeNode
    {
        PayloadIdType? IdType { get; set; }

        Task<object> CallChild(object value, Func<object, TreeNodeType, Task<object>> nextAsync);

        Type GetTValue();
    }
}
