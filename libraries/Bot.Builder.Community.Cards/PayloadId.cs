using System;

namespace Bot.Builder.Community.Cards.Management
{
    public class PayloadId : IEquatable<PayloadId>
    {
        public PayloadId(string type, string id)
        {
            Type = type ?? throw new ArgumentNullException(nameof(id));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public string Type { get; }

        public string Id { get; }

        public static bool operator ==(PayloadId left, PayloadId right) => left.Equals(right);

        public static bool operator !=(PayloadId left, PayloadId right) => !left.Equals(right);

        public override int GetHashCode() => (Type, Id).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as PayloadId);

        public bool Equals(PayloadId other) => Type == other?.Type && Id == other?.Id;
    }
}
