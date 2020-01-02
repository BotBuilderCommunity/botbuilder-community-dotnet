using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards
{
    public class RecursiveIdCollection
    {
        internal RecursiveIdCollection(IDictionary<IdType, string> ids)
        {
            Ids = ids ?? new Dictionary<IdType, string>();
        }

        internal RecursiveIdCollection(IList<RecursiveIdCollection> collection = null)
        {
            Collection = collection ?? new List<RecursiveIdCollection>();
        }

        public IDictionary<IdType, string> Ids { get; }

        public IList<RecursiveIdCollection> Collection { get; }

        public async Task RecurseOverDictionariesAsync(Func<IDictionary<IdType, string>, Task> func)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            if (Ids != null)
            {
                await func(Ids).ConfigureAwait(false);
            }

            if (Collection != null)
            {
                foreach (var recursiveId in Collection)
                {
                    await recursiveId.RecurseOverDictionariesAsync(func).ConfigureAwait(false);
                }
            }
        }

        public async Task RecurseOverIdsAsync(Func<string, IdType, Task> func)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            await RecurseOverDictionariesAsync(async dict =>
            {
                foreach (var kvp in dict)
                {
                    await func(kvp.Value, kvp.Key).ConfigureAwait(false);
                } 
            }).ConfigureAwait(false);
        }

        public IDictionary<IdType, ISet<string>> FlattenIntoDictionary()
        {
            var dict = Helper.NewIdDictionary();

            RecurseOverIdsAsync((id, type) =>
            {
                dict[type].Add(id);

                return Task.CompletedTask;
            }).Wait();

            return dict;
        }

        public ISet<string> FlattenIntoSet()
        {
            var set = new HashSet<string>();

            RecurseOverIdsAsync((id, _) =>
            {
                set.Add(id);

                return Task.CompletedTask;
            }).Wait();

            return set;
        }
    }
}