using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards
{
    public static class CardExtensions
    {
        public static IEnumerable<JToken> NonDataDescendants(this JContainer container) =>
            container?.Descendants().Where(token =>
                !token.Ancestors().Any(ancestor =>
                    (ancestor as JProperty)?.Name.EqualsCI(
                        CardConstants.KeyData) == true));

        public static IEnumerable<JObject> GetAdaptiveInputs(this JContainer container)
        {
            var inputTypes = new[] { "Input.Text", "Input.Number", "Input.Date", "Input.Time", "Input.Toggle", "Input.ChoiceSet" };

            return container?.NonDataDescendants()
                .Select(token => token is JObject element
                    && inputTypes.Contains(element.GetValueCI(CardConstants.KeyType)?.ToString())
                    && element.GetValueCI(CardConstants.KeyId) != null ? element : null)
                .WhereNotNull();
        }

        public static string GetAdaptiveInputId(this JObject input) => input?.GetValueCI(CardConstants.KeyId)?.ToString();
    }
}