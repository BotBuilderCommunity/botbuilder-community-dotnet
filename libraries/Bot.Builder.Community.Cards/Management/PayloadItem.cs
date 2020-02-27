using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class PayloadItem : IEquatable<PayloadItem>
    {
        public PayloadItem(string path, string value)
        {
            Path = string.IsNullOrWhiteSpace(path) ? throw new ArgumentNullException(nameof(path)) : path.Trim();
            Value = value;
        }

        public string Path { get; }

        public string Value { get; }

        public static bool operator ==(PayloadItem left, PayloadItem right) => left.Equals(right);

        public static bool operator !=(PayloadItem left, PayloadItem right) => !left.Equals(right);

        public override int GetHashCode() => (Path, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as PayloadItem);

        public bool Equals(PayloadItem other) => Path == other?.Path && Value == other?.Value;
    }
}
