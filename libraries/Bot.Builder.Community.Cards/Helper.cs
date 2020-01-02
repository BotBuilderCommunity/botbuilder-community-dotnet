using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards
{
    internal static class Helper
    {
        internal static IEnumerable<T> GetEnumValues<T>()
            where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        internal static IDictionary<IdType, ISet<string>> NewIdDictionary()
        {
            return GetEnumValues<IdType>().ToDictionary(type => type, _ => new HashSet<string>() as ISet<string>);
        }
    }
}