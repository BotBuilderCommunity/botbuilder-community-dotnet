using System;
using System.Collections.ObjectModel;

namespace Bot.Builder.Community.Cards.Management
{
    public class DataId : DataItem<string>
    {
        /// <summary>
        /// An action data ID that can identify the action, card, carousel, or batch the data came from.
        /// </summary>
        public DataId(string key, string value) : base(key, value)
        {
        }

        internal static ReadOnlyCollection<string> Scopes { get; } = Array.AsReadOnly(new[]
        {
            DataIdScopes.Action,
            DataIdScopes.Card,
            DataIdScopes.Carousel,
            DataIdScopes.Batch,
        });

        internal static string GenerateValue(string scope) => $"{scope}-{Guid.NewGuid()}";
    }
}
