using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class PayloadItem : IEquatable<PayloadItem>
    {
        public PayloadItem(string key, string value)
        {
            Key = string.IsNullOrWhiteSpace(key) ? throw new ArgumentNullException(nameof(key)) : key.Trim();
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }

        public static bool operator ==(PayloadItem left, PayloadItem right) => left.Equals(right);

        public static bool operator !=(PayloadItem left, PayloadItem right) => !left.Equals(right);

        public override int GetHashCode() => (Key, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as PayloadItem);

        public bool Equals(PayloadItem other) => Key == other?.Key && Value == other?.Value;
    }
}
