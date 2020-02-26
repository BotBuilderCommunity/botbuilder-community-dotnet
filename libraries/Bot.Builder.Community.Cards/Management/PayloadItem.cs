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

        public static ISet<PayloadItem> CreateIdSet(string idType, string value = null) => new HashSet<PayloadItem> { CreateId(idType, value) };

        public static PayloadItem CreateId(string idType, string value = null) => new PayloadItem($"{CardConstants.PrefixPayloadIds}{idType}", value);

        public override int GetHashCode() => (Path, Value).GetHashCode();

        public override bool Equals(object obj) => Equals(obj as PayloadItem);

        public bool Equals(PayloadItem other) => Path == other?.Path && Value == other?.Value;

        public string GetIdType()
        {
            if (Path.StartsWith(CardConstants.PrefixPayloadIds))
            {
                var type = Path.Substring(CardConstants.PrefixPayloadIds.Length);

                if (PayloadIdTypes.Collection.Contains(type))
                {
                    return type;
                }
            }

            return null;
        }
    }
}
