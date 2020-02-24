using System;

namespace Bot.Builder.Community.Cards.Management
{
    public class PayloadId : IEquatable<PayloadId>
    {
        public PayloadId(string type, string value)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Type { get; }

        public string Value { get; }

        public static bool operator ==(PayloadId left, PayloadId right) => left.Equals(right);

        public static bool operator !=(PayloadId left, PayloadId right) => !left.Equals(right);

        public override int GetHashCode() => (Type, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as PayloadId);

        public bool Equals(PayloadId other) => Type == other?.Type && Value == other?.Value;
    }
}
