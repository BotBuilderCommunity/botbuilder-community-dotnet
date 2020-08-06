using System;
using System.Collections.ObjectModel;

namespace Bot.Builder.Community.Cards.Management
{
    internal static class DataId
    {
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
