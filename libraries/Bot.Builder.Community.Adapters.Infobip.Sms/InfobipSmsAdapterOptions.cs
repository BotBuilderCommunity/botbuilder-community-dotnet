using Bot.Builder.Community.Adapters.Infobip.Core;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Adapters.Infobip.Sms
{
    public class InfobipSmsAdapterOptions: InfobipAdapterOptionsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipSmsAdapterOptions"/> class using appsettings.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <remarks>
        /// The configuration keys are:
        ///     InfobipApiKey: An Infobip API key.
        ///     InfobipApiBaseUrl: The Infobip base url.
        ///     InfobipAppSecret: A secret used to validate that incoming webhooks are originated from Infobip.
        ///     InfobipSmsNumber: A Sms assigned number.
        ///     InfobipSmsScenarioKey: A scenario key used to send SMS messages through OMNI failover API.
        /// </remarks>
        public InfobipSmsAdapterOptions(IConfiguration configuration)
            : this(configuration["InfobipApiKey"],
                configuration["InfobipApiBaseUrl"],
                configuration["InfobipAppSecret"],
                configuration["InfobipSmsNumber"],
                configuration["InfobipSmsScenarioKey"])
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipSmsAdapterOptions"/> class.
        /// </summary>
        /// <param name="apiKey">An Infobip API key.</param>
        /// <param name="apiBaseUrl">The Infobip base url.</param>
        /// <param name="appSecret">A secret used to validate that incoming webhooks are originated from Infobip.</param>
        /// <param name="smsNumber">A SMS assigned number.</param>
        /// <param name="smsScenarioKey">A scenario key used to send SMS messages through OMNI failover API.</param>
        public InfobipSmsAdapterOptions(string apiKey, string apiBaseUrl, string appSecret, string smsNumber, string smsScenarioKey): base(apiKey, apiBaseUrl, appSecret)
        {
            InfobipSmsNumber = smsNumber;
            InfobipSmsScenarioKey = smsScenarioKey;
        }

        /// <summary>
        /// Gets or Sets the SMS assigned number.
        /// </summary>
        /// <value>The SMS number.</value>
        public string InfobipSmsNumber { get; private set; }

        /// <summary>
        /// Gets or Sets the scenario key of scenario which will be used for sending through OMNI failover SMS messages.
        /// </summary>
        /// <value>The scenario key.</value>
        public string InfobipSmsScenarioKey { get; private set; }
    }
}