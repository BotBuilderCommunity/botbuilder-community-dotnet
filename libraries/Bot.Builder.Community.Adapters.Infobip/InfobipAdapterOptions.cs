using System;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Adapters.Infobip
{
    public class InfobipAdapterOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipAdapterOptions"/> class using appsettings.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <remarks>
        /// The configuration keys are:
        ///     InfobipApiKey: An Infobip API key.
        ///     InfobipApiBaseUrl: The Infobip base url.
        ///     InfobipAppSecret: A secret used to validate that incoming webhooks are originated from Infobip.
        ///     InfobipWhatsAppNumber: A WhatsApp assigned number.
        ///     InfobipScenarioKey: A scenario key used to send messages through OMNI failover API.
        /// </remarks>
        public InfobipAdapterOptions(IConfiguration configuration)
            : this(configuration["InfobipApiKey"], configuration["InfobipApiBaseUrl"], configuration["InfobipAppSecret"], configuration["InfobipWhatsAppNumber"], configuration["InfobipScenarioKey"])
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipAdapterOptions"/> class.
        /// </summary>
        /// <param name="apiKey">An Infobip API key.</param>
        /// <param name="apiBaseUrl">The Infobip base url.</param>
        /// <param name="appSecret">A secret used to validate that incoming webhooks are originated from Infobip.</param>
        /// <param name="whatsAppNumber">A WhatsApp assigned number.</param>
        /// <param name="scenarioKey">A scenario key used to send messages through OMNI failover API.</param>
        public InfobipAdapterOptions(string apiKey, string apiBaseUrl, string appSecret, string whatsAppNumber, string scenarioKey)
        {
            InfobipApiKey = apiKey;
            InfobipApiBaseUrl = apiBaseUrl;
            InfobipAppSecret = appSecret;
            InfobipWhatsAppNumber = whatsAppNumber;
            InfobipScenarioKey = scenarioKey;
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
        /// Gets or Sets the WhatsApp assigned number.
        /// </summary>
        /// <value>The WhatsApp number.</value>
        public string InfobipWhatsAppNumber { get; set; }

        /// <summary>
        /// Gets or Sets the scenario key of scenario which will be used for sending through OMNI failover messages.
        /// </summary>
        /// <value>The scenario key.</value>
        public string InfobipScenarioKey { get; set; }

        /// <summary>
        /// Should authentication be bypassed. This should never be set in production but can be useful when debugging your bot locally.
        /// </summary>
        public bool BypassAuthentication { get; set; } = false;
    }
}