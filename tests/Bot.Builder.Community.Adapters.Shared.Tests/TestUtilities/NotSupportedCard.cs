namespace Bot.Builder.Community.Adapters.Shared.Tests.TestUtilities
{
    public class NotSupportedCard
    {
        public const string ContentType = "application/vnd.microsoft.card.notsupportedcard";

        /// <summary>
        /// The title. This is a different type from other card's Title field so it causes deserialization errors.
        /// </summary>
        public string[] Title = new[] { "Not supported card", "and wrong type" };
    }
}
