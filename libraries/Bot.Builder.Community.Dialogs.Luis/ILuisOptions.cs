namespace Bot.Builder.Community.Dialogs.Luis
{
    /// <summary>
    /// Interface containing optional parameters for a LUIS request.
    /// </summary>
    public interface ILuisOptions
    {
        /// <summary>
        /// Gets or sets the value indicating if logging of queries to LUIS is allowed.
        /// </summary>
        /// <value>
        /// The value indicating if logging of queries to LUIS is allowed.
        /// </value>
        bool? Log { get; set; }

        /// <summary>
        /// Gets or sets the value indicating if spell checking is enabled.
        /// </summary>
        /// <value>
        /// Turn on spell checking.
        /// </value>
        bool? SpellCheck { get; set; }

        /// <summary>
        /// Gets or sets the value indicating if staging endpoint should be used.
        /// </summary>
        /// <value>
        /// The value indicating if staging endpoint should be used.
        /// </value>
        bool? Staging { get; set; }

        /// <summary>
        /// Gets or sets the time zone offset.
        /// </summary>
        /// <value>
        /// The time zone offset.
        /// </value>
        double? TimezoneOffset { get; set; }

        /// <summary>
        /// Gets or sets the verbose flag.
        /// </summary>
        /// <value>
        /// The verbose flag.
        /// </value>
        bool? Verbose { get; set; }

        /// <summary>
        /// Gets or sets the Bing Spell Check subscription key.
        /// </summary>
        /// <value>
        /// The Bing Spell Check subscription key.
        /// </value>
        string BingSpellCheckSubscriptionKey { get; set; }
    }

    public static partial class Extensions
    {
        public static void Apply(this ILuisOptions source, ILuisOptions target)
        {
            if (source.Log.HasValue)
            {
                target.Log = source.Log.Value;
            }

            if (source.SpellCheck.HasValue)
            {
                target.SpellCheck = source.SpellCheck.Value;
            }

            if (source.Staging.HasValue)
            {
                target.Staging = source.Staging.Value;
            }

            if (source.TimezoneOffset.HasValue)
            {
                target.TimezoneOffset = source.TimezoneOffset.Value;
            }

            if (source.Verbose.HasValue)
            {
                target.Verbose = source.Verbose.Value;
            }

            if (!string.IsNullOrWhiteSpace(source.BingSpellCheckSubscriptionKey))
            {
                target.BingSpellCheckSubscriptionKey = source.BingSpellCheckSubscriptionKey;
            }
        }
    }
}