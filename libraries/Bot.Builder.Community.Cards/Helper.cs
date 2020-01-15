using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards
{
    internal static class Helper
    {
        internal static IEnumerable<T> GetEnumValues<T>()
            where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}