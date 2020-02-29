using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards
{
    internal static class AdaptiveCardUtil
    {
        private static readonly string[] InputTypes = new[]
        {
            "Input.Text",
            "Input.Number",
            "Input.Date",
            "Input.Time",
            "Input.Toggle",
            "Input.ChoiceSet"
        };

        internal static IEnumerable<JToken> NonDataDescendants(JContainer container) =>
            container?.Descendants().Where(token =>
                !token.Ancestors().Any(ancestor =>
                    (ancestor as JProperty)?.Name.Equals(
                        CardConstants.KeyData) == true));

        internal static IEnumerable<JObject> GetAdaptiveInputs(JContainer container)
        {
            return NonDataDescendants(container)?
                .Select(token => token is JObject element
                    && InputTypes.Contains(element.GetValue(CardConstants.KeyType)?.ToString())
                    && element.GetValue(CardConstants.KeyId) != null ? element : null)
                .WhereNotNull();
        }

        internal static string GetAdaptiveInputId(JObject input) => input?.GetValue(CardConstants.KeyId)?.ToString();
    }
}