using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Cards
{
    public static class GenericExtensions
    {
        public static async Task<T> GetNonNullAsync<T>(
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

            var state = await statePropertyAccessor.GetAsync(turnContext, defaultValueFactory, cancellationToken);

            if (state == null && defaultValueFactory != null)
            {
                await statePropertyAccessor.SetAsync(turnContext, state = defaultValueFactory());
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
        public static bool InitializeKey<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value = default)
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

        public static bool IsOneOf<T>(this T obj, params T[] list)
        {
            return list.Contains(obj);
        }

        public static bool IsOneOf<T>(this T obj, IEqualityComparer<T> equalityComparer, params T[] list)
        {
            return list.Contains(obj, equalityComparer);
        }
    }
}
