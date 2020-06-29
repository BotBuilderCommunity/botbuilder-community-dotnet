using System.Net.Http;

namespace Bot.Builder.Community.Cards.Translation
{
    public class MicrosoftTranslatorConfig
    {
        public MicrosoftTranslatorConfig(string subscriptionKey, string targetLocale = null, HttpClient httpClient = null)
        {
            SubscriptionKey = subscriptionKey;
            TargetLocale = targetLocale;
            HttpClient = httpClient;
        }

        /// <summary>
        /// Sets the key used for the Cognitive Services translator API.
        /// </summary>
        /// <value>
        /// The key used for the Cognitive Services translator API.
        /// </value>
        public string SubscriptionKey { get; set; }

        public string TargetLocale { get; set; }

        public HttpClient HttpClient { get; set; }
    }
}
