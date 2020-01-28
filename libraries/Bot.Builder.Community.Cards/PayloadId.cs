using System;

namespace Bot.Builder.Community.Cards
{
    public class PayloadId
    {
        public PayloadId(PayloadIdType type, string id)
        {
            Type = type.CheckIdType();
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public PayloadIdType Type { get; }

        public string Id { get; }
    }
}
