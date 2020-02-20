using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal interface ITreeNode
    {
        string IdType { get; set; }

        Task<object> CallChild(object value, Func<object, TreeNodeType, Task<object>> nextAsync);

        Type GetTValue();
    }
}
