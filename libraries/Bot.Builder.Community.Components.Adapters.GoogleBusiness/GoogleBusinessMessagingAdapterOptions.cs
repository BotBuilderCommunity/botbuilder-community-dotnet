namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness
{
    public class GoogleBusinessMessagingAdapterOptions
    {
        public bool ValidateIncomingRequests { get; set; } = true;
        public string JsonKeyFile { get; set; } = "";
        public string PartnerKey { get; set; } = "";
    }
}
