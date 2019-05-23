using System.Linq;

namespace Bot.Builder.Community.Adapters.Twitter
{

    /// <summary>
    /// Required for any action, this represents the current user context.
    /// </summary>
    public class TwitterOptions
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }
        public string Environment { get; set; }
        public TwitterAccountApi Tier { get; set; }
        public string WebhookUri { get; set; }
        public string BotUsername { get; set; }
        public string[] AllowedUsernames { get; set; }

        /// <summary>
        /// Check if the current auth context is valid or not.
        /// Null or Empty value for one on the auth properties will return false.
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrEmpty(ConsumerKey) &&
            !string.IsNullOrEmpty(ConsumerSecret) &&
            !string.IsNullOrEmpty(AccessToken) &&
            !string.IsNullOrEmpty(AccessSecret) &&
            !string.IsNullOrEmpty(WebhookUri) &&
            (!string.IsNullOrEmpty(Environment) || Tier != TwitterAccountApi.PremiumFree);

        public bool AllowedUsernamesConfigured() => AllowedUsernames != null && AllowedUsernames.Any();
    }

    public enum TwitterAccountApi
    {
        PremiumFree = 0,
        PremiumPaid,
        Enterprise
    }
}
