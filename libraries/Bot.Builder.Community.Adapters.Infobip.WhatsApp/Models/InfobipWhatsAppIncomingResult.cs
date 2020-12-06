using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models
{
    public class InfobipWhatsAppIncomingResult: InfobipIncomingResultBase
    {
        [JsonProperty("message")] public InfobipIncomingWhatsAppMessage Message { get; set; }
        [JsonProperty("contact")] public InfobipIncomingWhatsAppContact Contact { get; set; }

        /// <summary>
        /// when message was seen in ISO8601 date time format
        /// </summary>
        [JsonProperty("seenAt")] public DateTimeOffset? SeenAt { get; set; }

        /// <summary>
        /// Returns True if this message represents seen report:
        /// https://dev-old.infobip.com/whatsapp-business-messaging/delivery-and-seen-reports
        /// </summary>
        /// <returns>True if this message represents seen report</returns>
        public bool IsSeenReport()
        {
            return SeenAt != null;
        }

        /// <summary>
        /// Returns True if this message represents message sent by subscriber to bot:
        /// https://dev-old.infobip.com/whatsapp-business-messaging/incoming-whatsapp-messages
        /// </summary>
        /// <returns>True if this message represents message sent by subscriber to bot</returns>
        public bool IsWhatsAppMessage()
        {
            return Message != null;
        }
    }

    public class InfobipIncomingWhatsAppContact
    {
        [JsonProperty("name")] public string Name { get; set; }
    }

    public class InfobipIncomingWhatsAppMessage
    {
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("url")] public Uri Url { get; set; }
        [JsonProperty("caption")] public string Caption { get; set; }
        [JsonIgnore] public byte[] Attachment { get; set; }
        [JsonProperty("longitude")] public double Longitude { get; set; }
        [JsonProperty("latitude")] public double Latitude { get; set; }

        /// <summary>
        /// Returns True if this message represents media message sent by subscriber to bot:
        /// https://dev-old.infobip.com/whatsapp-business-messaging/incoming-whatsapp-messages
        /// </summary>
        /// <returns>True if this message represents media message sent by subscriber to bot</returns>
        public bool IsMedia()
        {
            var mediaTypes = new[]
            {
                InfobipIncomingMessageTypes.Audio, InfobipIncomingMessageTypes.Document,
                InfobipIncomingMessageTypes.Image, InfobipIncomingMessageTypes.Video, InfobipIncomingMessageTypes.Voice
            };

            return mediaTypes.Contains(Type);
        }
    }
}