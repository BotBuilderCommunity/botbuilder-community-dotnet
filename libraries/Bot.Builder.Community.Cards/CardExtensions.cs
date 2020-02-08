using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards
{
    public static class CardExtensions
    {
        public static void ApplyIdsToPayload(this JObject payload, PayloadIdOptions options = null)
        {
            if (payload is null)
            {
                return;
            }

            if (options is null)
            {
                options = new PayloadIdOptions(PayloadIdType.Action);
            }

            foreach (var kvp in options.GetIds())
            {
                var type = kvp.Key;

                if (options.Overwrite || payload.GetIdFromPayload(type) is null)
                {
                    var id = kvp.Value;

                    if (id is null)
                    {
                        if (type == PayloadIdType.Action)
                        {
                            // Only generate an ID for the action
                            id = PayloadIdType.Action.GenerateId();
                        }
                        else
                        {
                            // If any other ID's are null,
                            // don't apply them to the payload
                            continue;
                        }
                    }

                    payload[type.GetKey()] = id;
                }
            }
        }

        public static string GetIdFromPayload(this JObject payload, PayloadIdType type = PayloadIdType.Card) =>
            payload?.GetValueCI(type.GetKey()) is JToken id ? id.ToString() : null;

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

        public static string GetAdaptiveInputId(this JObject input) =>
            input?.GetValueCI(CardConstants.KeyId)?.ToString();

        internal static string GetKey(this PayloadIdType type) => $"{CardConstants.PackageId}{type}Id";

        internal static string GenerateId(this PayloadIdType type) => $"{type}-{Guid.NewGuid()}";

        internal static string ReplaceNullWithId(this PayloadIdType type, ref PayloadIdOptions options)
        {
            options = options?.Clone() ?? new PayloadIdOptions();

            if (options.HasIdType(type))
            {
                var id = options.Get(type);

                return id is null ? options.Set(type, type.GenerateId()) : id;
            }

            return null;
        }

        internal static PayloadIdType CheckIdType(this PayloadIdType type)
        {
            if (!Enum.IsDefined(typeof(PayloadIdType), type))
            {
                throw new ArgumentException("The payload ID type must be defined.", nameof(type));
            }

            return type;
        }

        internal static IEnumerable<PayloadIdType> CheckIdTypes(this IEnumerable<PayloadIdType> types)
        {
            foreach (var type in types)
            {
                if (!Enum.IsDefined(typeof(PayloadIdType), type))
                {
                    throw new ArgumentException("Each payload ID type must be defined.");
                }
            }

            return types;
        }
    }
}