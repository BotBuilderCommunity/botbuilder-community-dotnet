using System;
using System.Collections.Generic;
using System.Text;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Adapters.Infobip.Viber
{
    public class InfobipViberAdapterOptions: InfobipAdapterOptionsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipViberAdapterOptions"/> class using appsettings.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <remarks>
        /// The configuration keys are:
        ///     InfobipApiKey: An Infobip API key.
        ///     InfobipApiBaseUrl: The Infobip base url.
        ///     InfobipAppSecret: A secret used to validate that incoming webhooks are originated from Infobip.
        ///     InfobipViberNumber: A Viber verified sender.
        ///     InfobipViberScenarioKey: A scenario key used to send Viber messages through OMNI failover API.
        /// </remarks>
        public InfobipViberAdapterOptions(IConfiguration configuration)
            : this(configuration["InfobipApiKey"],
                configuration["InfobipApiBaseUrl"],
                configuration["InfobipAppSecret"],
                configuration["InfobipViberSender"],
                configuration["InfobipViberScenarioKey"])
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipViberAdapterOptions"/> class.
        /// </summary>
        /// <param name="apiKey">An Infobip API key.</param>
        /// <param name="apiBaseUrl">The Infobip base url.</param>
        /// <param name="appSecret">A secret used to validate that incoming webhooks are originated from Infobip.</param>
        /// <param name="viberSender">A Viber verified sender.</param>
        /// <param name="viberScenarioKey">A scenario key used to send Viber messages through OMNI failover API.</param>
        public InfobipViberAdapterOptions(string apiKey, string apiBaseUrl, string appSecret, string viberSender, string viberScenarioKey) : base(apiKey, apiBaseUrl, appSecret)
        {
            InfobipViberSender = viberSender;
            InfobipViberScenarioKey = viberScenarioKey;
        }

        /// <summary>
        /// Gets or Sets the Viber verified sender.
        /// </summary>
        /// <value>The Viber sender.</value>
        public string InfobipViberSender { get; private set; }

        /// <summary>
        /// Gets or Sets the scenario key of scenario which will be used for sending through OMNI failover Viber messages.
        /// </summary>
        /// <value>The scenario key.</value>
        public string InfobipViberScenarioKey { get; private set; }
    }
}
