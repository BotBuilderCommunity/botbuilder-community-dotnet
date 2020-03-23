namespace Bot.Builder.Community.Adapters.RingCentral
{
    public class RingCentralOptions
    {
        /// <summary>
        /// The Bot Application ID
        /// </summary>
        public string BotId { get; set; }

        /// <summary>
        /// Microsoft Application Id
        /// </summary>
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// The API host base path
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// The RingCentral tenant API url eg: https://[TENANT].api.engagement.dimelo.com/1.0
        /// </summary>
        public string RingCentralEngageApiUrl { get; set; }

        /// <summary>
        /// The RingCentral API Access Token
        /// </summary>
        public string RingCentralEngageApiAccessToken { get; set; }
        
        /// <summary>
        /// Flag to set logging all conversations with the bot to RingCentral
        /// Used in order to store full transcripts in RingCentral Engage
        /// </summary>
        public bool LogMessagesToRingCentral { get; set; }

        /// <summary>
        /// RingCentral Custom Source SDK ie. Bot - this is the URL that RingCentral exposes
        /// as a realtime endpoint URL eg: https://[TENANT].engagement.dimelo.com/realtime/sdk/5dc206a10e69dc66c5b55f21
        /// </summary>
        public string RingCentralEngageCustomSourceRealtimeEndpointUrl { get; set; }

        /// <summary>
        /// RingCentral Custom Source ie. Bot API Access Token
        /// </summary>
        public string RingCentralEngageCustomSourceApiAccessToken { get; set; }

        /// <summary>
        /// RingCentral Webhook verify token
        /// </summary>
        public string RingCentralEngageInterventionWebhookValidationToken { get; set; }

        /// <summary>
        /// RingCentral category that is being set on threads, where the bot is 
        /// controlling all the answers (no active human intervention). 
        /// The Category ID can be retrieved from the RingCentral admin platform
        /// and is of the following format: 2ac373750e69dc46c639ac79
        /// </summary>
        public string RingCentralEngageBotControlledThreadCategoryId { get; set; }

        /// <summary>
        /// RingCentral category that is being set on threads, where an agent is 
        /// engaging on. 
        /// The Category ID can be retrieved from the RingCentral admin platform
        /// and is of the following format: 2ac373750e69dc46c639ac79
        /// </summary>
        public string RingCentralEngageAgentControlledThreadCategoryId { get; set; }
    }
}
