using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal interface ITreeNode
    {
        string IdScope { get; set; }

        object CallChild(object value, Func<object, TreeNodeType, object> next, bool reassignChildren);

        Type GetTValue();
    }
}
