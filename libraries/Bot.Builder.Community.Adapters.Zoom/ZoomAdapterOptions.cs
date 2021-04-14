using System.ComponentModel.DataAnnotations;

namespace Bot.Builder.Community.Adapters.Zoom
{
    public class ZoomAdapterOptions
    {
        public bool ValidateIncomingZoomRequests { get; set; } = true;

        [Required]
        public string ClientSecret { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string BotJid { get; set; }

        public string VerificationToken { get; set; }
    }
}
