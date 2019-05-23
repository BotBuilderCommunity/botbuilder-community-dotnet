namespace Bot.Builder.Community.Dialogs.Luis
{
    using System;
    using System.Collections.Generic;
    using Bot.Builder.Community.Dialogs.Luis.Models;

    /// <summary>
    /// An abtraction to map from a LUIS <see cref="EntityRecommendation"/> to specific CLR types.
    /// </summary>
    public interface IEntityToType
    {
        /// <summary>
        /// Try to map LUIS <see cref="EntityRecommendation"/> instances to a <see cref="TimeSpan"/>, relative to now.
        /// </summary>
        /// <param name="now">The now reference <see cref="DateTime"/>.</param>
        /// <param name="entities">A list of possibly-relevant <see cref="EntityRecommendation"/> instances.</param>
        /// <param name="span">The output <see cref="TimeSpan"/>.</param>
        /// <returns>True if the mapping may have been successful, false otherwise.</returns>
        bool TryMapToTimeSpan(DateTime now, IEnumerable<EntityRecommendation> entities, out TimeSpan span);

        /// <summary>
        /// Try to map LUIS <see cref="EntityRecommendation"/> instances to a list of <see cref="DateTime"/> ranges, relative to now.
        /// </summary>
        /// <param name="now">The now reference <see cref="DateTime"/>.</param>
        /// <param name="entities">A list of possibly-relevant <see cref="EntityRecommendation"/> instances.</param>
        /// <param name="ranges">The output <see cref="DateTime"/> ranges.</param>
        /// <returns>True if the mapping may have been successful, false otherwise.</returns>
        bool TryMapToDateRanges(DateTime now, IEnumerable<EntityRecommendation> entities, out IEnumerable<Range<DateTime>> ranges);
    }
}
