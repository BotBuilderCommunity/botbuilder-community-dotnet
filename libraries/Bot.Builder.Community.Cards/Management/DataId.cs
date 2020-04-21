using System;

namespace Bot.Builder.Community.Cards.Management
{
    public class DataId : DataItem, IEquatable<DataId>
    {
        public DataId(string type, string value)
            : base(value)
        {
            Type = string.IsNullOrWhiteSpace(type) ? throw new ArgumentNullException(nameof(type)) : type.Trim();
        }

        public string Type { get; }

        public override string Key => GetKey(Type);

        public static bool operator ==(DataId left, DataId right) => left.Equals(right);

        public static bool operator !=(DataId left, DataId right) => !left.Equals(right);

        public override int GetHashCode() => (Type, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as DataId);

        public bool Equals(DataId other) => Type == other?.Type && Value == other?.Value;

        internal static string GetKey(string type) => $"{Prefixes.DataIdKey}{type}";

        internal static string GenerateValue(string type) => $"{type}-{Guid.NewGuid()}";
    }
}
