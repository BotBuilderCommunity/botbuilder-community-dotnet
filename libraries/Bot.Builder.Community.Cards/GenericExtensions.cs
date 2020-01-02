using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Builder.Community.Cards
{
    /// <summary>
    /// These are general-purpose extension methods that aren't directly related to the library
    /// and so they're not made public.
    /// </summary>
    internal static class GenericExtensions
    {
        internal static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
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
        /// True if the value was added because the key was not present.
        /// False if the value was not added because the key was present.
        /// </returns>
        internal static bool InitializeKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value = default)
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (dict.ContainsKey(key))
            {
                return false;
            }

            dict.Add(key, value);

            return true;
        }

        internal static bool SetExistingValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value = default)
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

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
    }
}
