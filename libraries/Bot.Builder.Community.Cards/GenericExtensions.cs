using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards
{
    /// <summary>
    /// These are general-purpose extension methods that aren't directly related to the library
    /// and so they're not made public.
    /// </summary>
    internal static class GenericExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source)
            where T : class
        {
            return source.Where(x => x != null);
        }

        internal static ConfiguredTaskAwaitable<T> CoalesceAwait<T>(this Task<T> task)
            where T : class
        {
            return (task ?? Task.FromResult<T>(null)).ConfigureAwait(false);
        }

        internal static async Task<T> GetNotNullAsync<T>(
            this IStatePropertyAccessor<T> statePropertyAccessor,
            ITurnContext turnContext,
            Func<T> defaultValueFactory = null,
            CancellationToken cancellationToken = default)
        {
            if (statePropertyAccessor is null)
            {
                throw new ArgumentNullException(nameof(statePropertyAccessor));
            }

            if (turnContext is null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var state = await statePropertyAccessor.GetAsync(turnContext, defaultValueFactory, cancellationToken).ConfigureAwait(false);

            if (state == null && defaultValueFactory != null)
            {
                await statePropertyAccessor.SetAsync(turnContext, state = defaultValueFactory()).ConfigureAwait(false);
            }

            return state;
        }

        /// <summary>
        /// Sets a value in a dictionary only if the key is not already present.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key to potentially add to the dictionary.</param>
        /// <param name="value">The value to potentially add to the dictionary.</param>
        /// <returns>
        /// The value.
        /// </returns>
        internal static TValue InitializeKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value = default)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            dict.Add(key, value);

            return value;
        }

        internal static bool SetExistingValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value = default)
        {
            if (!dict.ContainsKey(key))
            {
                return false;
            }

            dict[key] = value;

            return true;
        }

        internal static bool IsOneOf<T>(this T obj, params T[] these)
        {
            return these.Contains(obj);
        }

        internal static bool IsOneOf<T>(this T obj, IEqualityComparer<T> equalityComparer, params T[] these)
        {
            return these.Contains(obj, equalityComparer);
        }

        internal static JObject TryParseJObject(this string inputString)
        {
            try
            {
                return JObject.Parse(inputString);
            }
            catch
            {
                return null;
            }
        }

        internal static JObject ToJObject<T>(this T input, bool shouldParseStrings = false)
        {
            JToken jToken = null;

            if (shouldParseStrings && input is string inputString)
            {
                jToken = TryParseJObject(inputString);
            }
            else if (input is JObject inputJObject)
            {
                jToken = inputJObject;
            }
            else if (input != null)
            {
                jToken = JToken.FromObject(input);
            }

            return jToken as JObject;
        }

        internal static async Task<T> ToJObjectAndBackAsync<T>(
            this T input,
            Func<JObject, Task> funcAsync,
            bool shouldParseStrings = false,
            bool returnNullForWrongType = false)
            where T : class
        {
            if (input.ToJObject(shouldParseStrings) is JObject jObject)
            {
                await funcAsync(jObject).ConfigureAwait(false);

                return input is string
                    ? JsonConvert.SerializeObject(jObject) as T
                    : input is JObject
                        ? jObject as T
                        : jObject.ToObject<T>();
            }
            else if (returnNullForWrongType)
            {
                return null;
            }

            return input;
        }

        internal static JToken GetValueCI(this JObject jObject, string key) => jObject?.GetValue(key, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Clears any properties with the same name before setting the value,
        /// thus performing a "case-insentive" set.
        /// </summary>
        /// <param name="jObject">A JObject.</param>
        /// <param name="key">The name of the property to set.</param>
        /// <param name="value">The value.</param>
        internal static void SetValueCI(this JObject jObject, string key, JToken value)
        {
            while (jObject.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var token))
            {
                token.Parent.Remove();
            }

            jObject[key] = value;
        }

        internal static bool EqualsCI(this string left, string right) => left?.Equals(right, StringComparison.OrdinalIgnoreCase) == true;

        internal static bool IsNullish(this JToken jToken) => jToken is null || jToken.Type.IsOneOf(JTokenType.None, JTokenType.Null, JTokenType.Undefined);
    }
}
