namespace Bot.Builder.Community.Cards
{
    public enum TrackingStyle
    {
        /// <summary>
        /// Don't track ID's in card manager state
        /// </summary>
        None,

        /// <summary>
        /// Track enabled ID's in card manager state
        /// </summary>
        TrackEnabled,

        /// <summary>
        /// Track disabled ID's in card manager state
        /// </summary>
        TrackDisabled,
    }
}