using System;

namespace Bot.Builder.Community.Cards.Management
{
    public class DataItem : IEquatable<DataItem>
    {
        public DataItem(string key, string value)
        {
            Key = string.IsNullOrWhiteSpace(key) ? throw new ArgumentNullException(nameof(key)) : key.Trim();
            Value = value;
        }

        internal DataItem(string value)
        {
            Value = value;
        }

        public virtual string Key { get; }

        public string Value { get; }

        public static bool operator ==(DataItem left, DataItem right) => left.Equals(right);

        public static bool operator !=(DataItem left, DataItem right) => !left.Equals(right);

        public override int GetHashCode() => (Key, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as DataItem);

        public bool Equals(DataItem other) => Key == other?.Key && Value == other?.Value;
    }
}
