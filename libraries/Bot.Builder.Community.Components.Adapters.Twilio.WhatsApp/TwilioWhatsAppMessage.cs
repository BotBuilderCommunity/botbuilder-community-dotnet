namespace Bot.Builder.Community.Components.Adapters.Twilio.WhatsApp
{
    public class TwilioWhatsAppMessage : TwilioMessage
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
    }
}