using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Cards.Nodes
{
    public class TypedId
    {
        public TypedId(IdType type, string id)
        {
            Type = type;
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public IdType Type { get; }

        public string Id { get; }
    }
}
