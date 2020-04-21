using System;

namespace Bot.Builder.Community.Cards.Management
{
    /// <summary>
    /// A string property in action data that may or may not be an action data ID.
    /// This class is currently only used as the base class for <see cref="DataId"/>
    /// but it allows for the possibility of future functionality that adds non-ID
    /// properties to action data.
    /// </summary>
    public class DataItem : IEquatable<DataItem>
    {
        public DataItem(string key, string value)
        {
            Name = string.IsNullOrWhiteSpace(key) ? throw new ArgumentNullException(nameof(key)) : key.Trim();
            Value = value;
        }

        public virtual string Key => Name;

        public string Value { get; }

        /// <summary>
        /// Gets the backing field for both the <see cref="Key"/> property and the
        /// <see cref="DataId.Type"/> property so that <see cref="DataId"/> doesn't have
        /// an unused field for its overridden <see cref="DataId.Key">Key</see> property.
        /// </summary>
        /// <value>
        /// The backing field for both the <see cref="Key"/> property and
        /// the <see cref="DataId.Type"/> property.
        /// </value>
        protected string Name { get; }

        public static bool operator ==(DataItem left, DataItem right) => left.Equals(right);

        public static bool operator !=(DataItem left, DataItem right) => !left.Equals(right);

        public override int GetHashCode() => (Key, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as DataItem);

        public bool Equals(DataItem other) => Key == other?.Key && Value == other?.Value;
    }
}
