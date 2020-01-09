using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Cards.Nodes
{
    public class IdTypeValue
    {
        public IdTypeValue(IdType type, string id)
        {
            Type = type;
            Id = id;
        }

        public IdType Type { get; }

        public string Id { get; }
    }
}
