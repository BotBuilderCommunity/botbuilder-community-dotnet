using System;
using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management
{
    /// <summary>
    /// An action data ID that can identify the action, card, carousel, or batch the data came from.
    /// This class represents a sort of key/value pair where the key can be accessed in a shortened
    /// form (the data ID type) or a longer form (the key). The reason the key is longer is so that it
    /// will be less likely to conflict with other property names in the action data.
    /// </summary>
    public class DataId : DataItem, IEquatable<DataId>
    {
        public DataId(string type, string value)
            : base(type, value)
        {
        }

        public string Type => Name;

        public override string Key => GetKey(Name);

        public static bool operator ==(DataId left, DataId right) => left.Equals(right);

        public static bool operator !=(DataId left, DataId right) => !left.Equals(right);

        public override int GetHashCode() => (Type, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as DataId);

        public bool Equals(DataId other) => Type == other?.Type && Value == other?.Value;

        internal static IList<string> Types { get; } = Array.AsReadOnly(new[]
        {
            DataIdTypes.Action,
            DataIdTypes.Card,
            DataIdTypes.Carousel,
            DataIdTypes.Batch
        });

        internal static string GetKey(string type) => $"{Prefixes.DataIdKey}{type}";

        internal static string GenerateValue(string type) => $"{type}-{Guid.NewGuid()}";
    }
}
