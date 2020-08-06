using System;

namespace Bot.Builder.Community.Cards.Management
{
    /// <summary>
    /// A string property in action data that may or may not be an action data ID.
    /// </summary>
    public class DataItem : IEquatable<DataItem>
    {
        public DataItem(string key, string value)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }

        public static bool operator ==(DataItem left, DataItem right) => left.Equals(right);

        public static bool operator !=(DataItem left, DataItem right) => !left.Equals(right);

        public override int GetHashCode() => (Key, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as DataItem);

        public bool Equals(DataItem other) => Key == other?.Key && Value == other?.Value;
    }
}
