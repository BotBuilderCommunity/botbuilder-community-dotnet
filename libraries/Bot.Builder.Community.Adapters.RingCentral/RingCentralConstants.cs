namespace Bot.Builder.Community.Adapters.RingCentral
{
    public class RingCentralConstants
    {
        public enum RingCentralHandledEvent
        {
            Unknown = 0, 
            VerifyWebhook = 10,
            Intervention = 20,
            ContentImported = 30,
            Action = 40
        }

        // Webhook validation request from RingCentral will contain this in querystring
        public const string HubModeSubscribe = "subscribe";
    }
}
