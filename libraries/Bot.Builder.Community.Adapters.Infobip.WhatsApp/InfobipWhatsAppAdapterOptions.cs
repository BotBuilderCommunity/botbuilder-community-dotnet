using Bot.Builder.Community.Adapters.Infobip.Core;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp
{
    public class InfobipWhatsAppAdapterOptions: InfobipAdapterOptionsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipWhatsAppAdapterOptions"/> class using appsettings.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <remarks>
        /// The configuration keys are:
        ///     InfobipApiKey: An Infobip API key.
        ///     InfobipApiBaseUrl: The Infobip base url.
        ///     InfobipAppSecret: A secret used to validate that incoming webhooks are originated from Infobip.
        ///     InfobipWhatsAppNumber: A WhatsApp assigned number.
        ///     InfobipWhatsAppScenarioKey: A scenario key used to send WhatsApp messages through OMNI failover API.
        /// </remarks>
        public InfobipWhatsAppAdapterOptions(IConfiguration configuration)
            : this(configuration["InfobipApiKey"],
                configuration["InfobipApiBaseUrl"],
                configuration["InfobipAppSecret"],
                configuration["InfobipWhatsAppNumber"],
                configuration["InfobipWhatsAppScenarioKey"])
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipWhatsAppAdapterOptions"/> class.
        /// </summary>
        /// <param name="apiKey">An Infobip API key.</param>
        /// <param name="apiBaseUrl">The Infobip base url.</param>
        /// <param name="appSecret">A secret used to validate that incoming webhooks are originated from Infobip.</param>
        /// <param name="whatsAppNumber">A WhatsApp assigned number.</param>
        /// <param name="whatsAppScenarioKey">A scenario key used to send WhatsApp messages through OMNI failover API.</param>
        public InfobipWhatsAppAdapterOptions(string apiKey, string apiBaseUrl, string appSecret, string whatsAppNumber, string whatsAppScenarioKey): base(apiKey, apiBaseUrl, appSecret)
        {
            InfobipWhatsAppNumber = whatsAppNumber;
            InfobipWhatsAppScenarioKey = whatsAppScenarioKey;
        }

        /// <summary>
        /// Gets or Sets the WhatsApp assigned number.
        /// </summary>
        /// <value>The WhatsApp number.</value>
        public string InfobipWhatsAppNumber { get; set; }

        /// <summary>
        /// Gets or Sets the scenario key of scenario which will be used for sending through OMNI failover WhatsApp messages.
        /// </summary>
        /// <value>The scenario key.</value>
        public string InfobipWhatsAppScenarioKey { get; set; }
    }
}