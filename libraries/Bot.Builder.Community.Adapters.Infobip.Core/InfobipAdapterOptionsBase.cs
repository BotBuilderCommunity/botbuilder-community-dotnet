namespace Bot.Builder.Community.Adapters.Infobip.Core
{
    public abstract class InfobipAdapterOptionsBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipAdapterOptionsBase"/> class.
        /// </summary>
        /// <param name="apiKey">An Infobip API key.</param>
        /// <param name="apiBaseUrl">The Infobip base url.</param>
        /// <param name="appSecret">A secret used to validate that incoming webhooks are originated from Infobip.</param>
        protected InfobipAdapterOptionsBase(string apiKey, string apiBaseUrl, string appSecret)
        {
            InfobipApiKey = apiKey;
            InfobipApiBaseUrl = apiBaseUrl;
            InfobipAppSecret = appSecret;
        }

        /// <summary>
        /// Gets or Sets the Infobip API key used for outgoing requests authorization.
        /// </summary>
        /// <value>The API key.</value>
        public string InfobipApiKey { get; set; }

        /// <summary>
        /// Gets or Sets the Infobip base url where outgoing requests will be sent.
        /// </summary>
        /// <value>The Base URL.</value>
        public string InfobipApiBaseUrl { get; set; }

        /// <summary>
        /// Gets or Sets the secret for validating the origin of incoming webhooks.
        /// </summary>
        /// <value>The App secret.</value>
        public string InfobipAppSecret { get; set; }

        /// <summary>
        /// Should authentication be bypassed. This should never be set in production but can be useful when debugging your bot locally.
        /// </summary>
        public bool BypassAuthentication { get; set; } = false;
    }
}
