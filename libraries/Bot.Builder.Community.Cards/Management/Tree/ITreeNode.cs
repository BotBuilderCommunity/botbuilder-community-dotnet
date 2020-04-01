using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal interface ITreeNode
    {
        string IdType { get; set; }

        Task<object> CallChildAsync(object value, Func<object, TreeNodeType, Task<object>> nextAsync, bool reassignChildren);

        Type GetTValue();
    }
}
