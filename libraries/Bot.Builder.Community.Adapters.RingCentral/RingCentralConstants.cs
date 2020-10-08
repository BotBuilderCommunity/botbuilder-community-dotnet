namespace Bot.Builder.Community.Adapters.RingCentral
{
    public static class RingCentralConstants
    {
        // Webhook validation request from RingCentral will contain this in querystring
        public const string HubModeSubscribe = "subscribe";

        /// <summary>
        /// Any 'event' coming from RingCentral that we want to act upon.
        /// </summary>
        public enum RingCentralHandledEvent
        {
            /// <summary>
            /// The RingCentral event is unhandled.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// The RingCentral event is a webhook verification check.
            /// </summary>
            VerifyWebhook = 10,

            /// <summary>
            /// The RingCentral event is an agent intervention.
            /// </summary>
            Intervention = 20,

            /// <summary>
            /// The RingCentral event is content.imported .
            /// </summary>
            ContentImported = 30,
            
            /// <summary>
            /// The RingCentral event is an action
            /// </summary>
            Action = 40
        }
    }
}
