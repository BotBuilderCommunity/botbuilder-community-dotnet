namespace Bot.Builder.Community.Adapters.Zoom
{
    public class ZoomAdapterOptions
    {
        public bool ValidateIncomingZoomRequests { get; set; } = true;
        public string ClientSecret { get; set; }
        public string ClientId { get; set; }
        public string BotJid { get; set; }
        public string VerificationToken { get; set; }
    }
}
