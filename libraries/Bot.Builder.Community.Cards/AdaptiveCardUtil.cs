using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards
{
    internal static class AdaptiveCardUtil
    {
        internal static IEnumerable<JToken> NonDataDescendants(JContainer container) =>
            container?.Descendants().Where(token =>
                !token.Ancestors().Any(ancestor =>
                    (ancestor as JProperty)?.Name.EqualsCI(
                        CardConstants.KeyData) == true));

        internal static IEnumerable<JObject> GetAdaptiveInputs(JContainer container)
        {
            var inputTypes = new[] { "Input.Text", "Input.Number", "Input.Date", "Input.Time", "Input.Toggle", "Input.ChoiceSet" };

            return NonDataDescendants(container)?
                .Select(token => token is JObject element
                    && inputTypes.Contains(element.GetValueCI(CardConstants.KeyType)?.ToString())
                    && element.GetValueCI(CardConstants.KeyId) != null ? element : null)
                .WhereNotNull();
        }

        internal static string GetAdaptiveInputId(JObject input) => input?.GetValueCI(CardConstants.KeyId)?.ToString();
    }
}