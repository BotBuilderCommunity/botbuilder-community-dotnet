namespace Bot.Builder.Community.Adapters.RingCentral
{
    public class RingCentralOptions
    {
        /// <summary>
        /// Gets or sets the Bot Application Name.
        /// </summary>
        /// <value>
        /// The registered bot name, from Azure Bot Service.
        /// </value>
        public string BotId { get; set; }

        /// <summary>
        /// Gets or sets microsoft Application Id.
        /// </summary>
        /// <value>
        /// The Microsoft Application Id.
        /// </value>
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets the RingCentral tenant API url eg: https://[TENANT].api.engagement.dimelo.com/1.0.
        /// </summary>
        /// <value>
        /// The RingCentral Engage API Url.
        /// </value>
        public string RingCentralEngageApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the RingCentral API Access Token.
        /// </summary>
        /// <value>
        /// The RingCentral Engage API Access Token.
        /// </value>/// 
        public string RingCentralEngageApiAccessToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether flag to set logging all conversations with the bot to RingCentral
        /// Used in order to store full transcripts in RingCentral Engage.
        /// </summary>
        /// <value>
        /// Log all bot messages to RingCentral flag.
        /// </value>
        public bool LogMessagesToRingCentral { get; set; }

        /// <summary>
        /// Gets or sets ringCentral Custom Source SDK ie. Bot - this is the URL that RingCentral exposes
        /// as a realtime endpoint URL eg: https://[TENANT].engagement.dimelo.com/realtime/sdk/5dc206a10e69dc66c5b55f21.
        /// </summary>
        /// <value>
        /// The RingCentral Engage Source SDK Realtime Endpoint Url.
        /// </value>
        public string RingCentralEngageCustomSourceRealtimeEndpointUrl { get; set; }

        /// <summary>
        /// Gets or sets ringCentral Custom Source ie. Bot API Access Token.
        /// </summary>
        /// <value>
        /// The RingCentral Engage Custom Source Api Access Token.
        /// </value>/// 
        public string RingCentralEngageCustomSourceApiAccessToken { get; set; }

        /// <summary>
        /// Gets or sets ringCentral Webhook verify token.
        /// </summary>
        /// <value>
        /// The RingCentral Engage Webhook Validation Token.
        /// </value>
        public string RingCentralEngageInterventionWebhookValidationToken { get; set; }

        /// <summary>
        /// Gets or sets ringCentral category that is being set on threads, where the bot is 
        /// controlling all the answers (no active human intervention). 
        /// The Category ID can be retrieved from the RingCentral admin platform
        /// and is of the following format: 2ac373750e69dc46c639ac79.
        /// </summary>
        /// <value>
        /// The RingCentral Engage Bot Category Id.
        /// </value>
        public string RingCentralEngageBotControlledThreadCategoryId { get; set; }

        /// <summary>
        /// Gets or sets ringCentral category that is being set on threads, where an agent is 
        /// engaging on. 
        /// The Category ID can be retrieved from the RingCentral admin platform
        /// and is of the following format: 2ac373750e69dc46c639ac79.
        /// </summary>
        /// <value>
        /// The RingCentral Engage Agent Category Id.
        /// </value>
        public string RingCentralEngageAgentControlledThreadCategoryId { get; set; }
    }
}
