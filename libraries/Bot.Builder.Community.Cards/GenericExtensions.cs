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
        internal static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source)
            where T : class
        {
            return source.Where(x => x != null);
        }

        internal static async Task<T> GetNotNullAsync<T>(
            this IStatePropertyAccessor<T> statePropertyAccessor,
            ITurnContext turnContext,
            Func<T> defaultValueFactory = null,
            CancellationToken cancellationToken = default)
        {
            if (statePropertyAccessor is null || turnContext is null)
            {
                return default;
            }

            var state = await (statePropertyAccessor?.GetAsync(turnContext, defaultValueFactory, cancellationToken)).ConfigureAwait(false);

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

        internal static JObject ToJObject(this object input, bool shouldParseStrings = false)
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

        /// <summary>
        /// Converts the input to a <see cref="JObject"/>,
        /// performs a function on it if the conversion was successful,
        /// converts the <see cref="JObject"/> back to the original type,
        /// and returns it asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the input and return value.</typeparam>
        /// <param name="input">The instance to convert to a <see cref="JObject">JObject</see> and back.</param>
        /// <param name="funcAsync">The function to perform on the <see cref="JObject">JObject</see>. The argument
        /// passed to this function is guaranteed to not be null, so no null checking is necessary.</param>
        /// <param name="shouldParseStrings">True if string input should be deserialized,
        /// false if a string should count as a wrong type.</param>
        /// <param name="returnNullForWrongType">True if null should be returned if the input couldn't be
        /// converted to a <see cref="JObject">JObject</see>, false if the original input should be returned if
        /// it couldn't be converted to a <see cref="JObject">JObject</see>.</param>
        /// <returns>The potentially-modified input after being converted back to <typeparamref name="T"/> if the
        /// conversion to a <see cref="JObject">JObject</see> was successful, or a value determined by
        /// <paramref name="returnNullForWrongType"/> if the conversion was unsuccessful.</returns>
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

                return (input is string
                        ? JsonConvert.SerializeObject(jObject)
                        : input is JObject
                            ? jObject
                            : jObject.ToObject(input.GetType()))
                    as T;
            }
            else if (returnNullForWrongType)
            {
                return null;
            }
            else
            {
                return input;
            }
        }

        internal static void SetValue(this JObject jObject, string key, JToken value)
        {
            jObject[key] = value;
        }

        internal static bool EqualsCI(this string left, string right) => left is null ? right is null : left.Equals(right, StringComparison.OrdinalIgnoreCase);

        internal static bool IsNullish(this JToken jToken) => jToken is null || jToken.Type.IsOneOf(JTokenType.None, JTokenType.Null, JTokenType.Undefined);

        internal static bool IsNullishOrEmpty(this JToken jToken) => jToken.IsNullish() || !jToken.Any();

        internal static void SafeRemove(this JToken jToken)
        {
            if (jToken?.Parent is JProperty parent && parent.Parent != null)
            {
                parent.Remove();
            }
            else if (jToken?.Parent != null)
            {
                jToken.Remove();
            }
        }

        internal static string SerializeIfNeeded(this object obj) => obj is null ? null : obj is string str ? str : JsonConvert.SerializeObject(obj);
    }
}
