namespace Bot.Builder.Community.Dialogs.Luis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Bot.Builder.Community.Dialogs.Luis.Models;
    using static Bot.Builder.Community.Dialogs.Luis.BuiltIn.DateTime;

    /// <summary>
    /// LUIS extension methods.
    /// </summary>
    public static partial class Extensions
    {
        public static IEnumerable<int> Enumerate(this Range<int> range)
        {
            for (int index = range.Start; index < range.After; ++index)
            {
                yield return index;
            }
        }

        public static T Min<T>(T one, T two)
            where T : IComparable<T>
        {
            var compare = one.CompareTo(two);
            return compare < 0 ? one : two;
        }

        public static IEnumerable<Range<T>> SortedMerge<T>(this IEnumerable<Range<T>> oneItems, IEnumerable<Range<T>> twoItems)
            where T : IEquatable<T>, IComparable<T>
        {
            using (var one = oneItems.GetEnumerator())
            using (var two = twoItems.GetEnumerator())
            {
                T oneIndex = default(T);
                T twoIndex = default(T);

                bool oneMore = one.MoveNext();
                bool twoMore = two.MoveNext();

                if (oneMore)
                {
                    oneIndex = one.Current.Start;
                }

                if (twoMore)
                {
                    twoIndex = two.Current.Start;
                }

                if (oneMore && twoMore)
                {
                    while (true)
                    {
                        var compare = oneIndex.CompareTo(twoIndex);
                        if (compare < 0)
                        {
                            var after = Min(one.Current.After, twoIndex);
                            oneMore = Advance(one, ref oneIndex, after);
                            if (!oneMore)
                            {
                                break;
                            }
                        }
                        else if (compare == 0)
                        {
                            var after = Min(one.Current.After, two.Current.After);
                            yield return new Range<T>(oneIndex, after);
                            oneMore = Advance(one, ref oneIndex, after);
                            twoMore = Advance(two, ref twoIndex, after);
                            if (!(oneMore && twoMore))
                            {
                                break;
                            }
                        }
                        else if (compare > 0)
                        {
                            var after = Min(two.Current.After, oneIndex);
                            twoMore = Advance(two, ref twoIndex, after);
                            if (!twoMore)
                            {
                                break;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Query the LUIS service using this text.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="text">The query text.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, string text, CancellationToken token)
        {
            var luisRequest = service.ModifyRequest(new LuisRequest(query: text));
            return await service.QueryAsync(luisRequest, token);
        }

        /// <summary>
        /// Query the LUIS service using this request.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="request">Query request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, LuisRequest request, CancellationToken token)
        {
            service.ModifyRequest(request);
            var uri = service.BuildUri(request);
            return await service.QueryAsync(uri, token);
        }

        /// <summary>
        /// Builds luis uri with text query.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="text">The query text.</param>
        /// <returns>The LUIS request Uri.</returns>
        public static Uri BuildUri(this ILuisService service, string text)
        {
            return service.BuildUri(service.ModifyRequest(new LuisRequest(query: text)));
        }

        public static T MaxBy<T, R>(this IEnumerable<T> items, Func<T, R> selectRank, IComparer<R> comparer = null)
        {
            comparer = comparer ?? Comparer<R>.Default;

            var bestItem = default(T);
            var bestRank = default(R);
            using (var item = items.GetEnumerator())
            {
                if (item.MoveNext())
                {
                    bestItem = item.Current;
                    bestRank = selectRank(item.Current);
                }

                while (item.MoveNext())
                {
                    var rank = selectRank(item.Current);
                    var compare = comparer.Compare(rank, bestRank);
                    if (compare > 0)
                    {
                        bestItem = item.Current;
                        bestRank = rank;
                    }
                }
            }

            return bestItem;
        }

        /// <summary>
        /// Try to find an entity within the result.
        /// </summary>
        /// <param name="result">The LUIS result.</param>
        /// <param name="type">The entity type.</param>
        /// <param name="entity">The found entity.</param>
        /// <returns>True if the entity was found, false otherwise.</returns>
        public static bool TryFindEntity(this LuisResult result, string type, out EntityRecommendation entity)
        {
            Func<EntityRecommendation, IList<EntityRecommendation>, bool> doesNotOverlapRange = (current, recommendations) =>
            {
                return !recommendations.Where(r => current != r)
                            .Any(r => r.StartIndex.HasValue && r.EndIndex.HasValue && current.StartIndex.HasValue &&
                                 r.StartIndex.Value <= current.StartIndex.Value && r.EndIndex.Value >= current.EndIndex.Value);
            };

            // find the recommended entity that does not overlap start and end ranges with other result entities
            entity = result.Entities?.Where(e => e.Type == type && doesNotOverlapRange(e, result.Entities)).FirstOrDefault();
            return entity != null;
        }

        /// <summary>
        /// Parse all resolutions from a LUIS result.
        /// </summary>
        /// <param name="parser">The resolution parser.</param>
        /// <param name="entities">The LUIS entities.</param>
        /// <returns>The parsed resolutions.</returns>
        public static IEnumerable<Resolution> ParseResolutions(this IResolutionParser parser, IEnumerable<EntityRecommendation> entities)
        {
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    Resolution resolution;
                    if (parser.TryParse(entity.Resolution, out resolution))
                    {
                        yield return resolution;
                    }
                }
            }
        }

        /// <summary>
        /// Return the next <see cref="DayPart"/>.
        /// </summary>
        /// <param name="part">The <see cref="DayPart"/> query.</param>
        /// <returns>The next <see cref="DayPart"/> after the query.</returns>
        public static DayPart Next(this DayPart part)
        {
            switch (part)
            {
                case DayPart.MO: return DayPart.MI;
                case DayPart.MI: return DayPart.AF;
                case DayPart.AF: return DayPart.EV;
                case DayPart.EV: return DayPart.NI;
                case DayPart.NI: return DayPart.MO;
                default: throw new NotImplementedException();
            }
        }

        private static bool Advance<T>(IEnumerator<Range<T>> enumerator, ref T index, T after)
            where T : IEquatable<T>, IComparable<T>
        {
            index = after;
            var compare = index.CompareTo(enumerator.Current.After);
            if (compare < 0)
            {
                return true;
            }
            else if (compare == 0)
            {
                bool more = enumerator.MoveNext();
                if (more)
                {
                    index = enumerator.Current.Start;
                }

                return more;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}