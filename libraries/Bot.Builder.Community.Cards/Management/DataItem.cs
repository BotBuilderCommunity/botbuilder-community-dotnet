using System;

namespace Bot.Builder.Community.Cards.Management
{
    /// <summary>
    /// A string property in action data that may or may not be an action data ID.
    /// </summary>
    public class DataItem<T> : IEquatable<DataItem<T>>
    {
        public DataItem(string key, T value)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value;
        }

        public string Key { get; }

        public T Value { get; }

        public static bool operator ==(DataItem<T> left, DataItem<T> right) => Equals(left, right);

        public static bool operator !=(DataItem<T> left, DataItem<T> right) => !Equals(left, right);

        public override int GetHashCode() => (Key, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as DataItem<T>);

        public bool Equals(DataItem<T> other) => !(other is null) && Equals(Key, other.Key) && Equals(Value, other.Value);
    }
}
