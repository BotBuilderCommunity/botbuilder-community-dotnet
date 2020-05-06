using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Models
{
    public class InfobipIncomingMessage
    {
        [JsonProperty("results")] public List<InfobipIncomingResult> Results { get; set; }
        [JsonProperty("messageCount")] public long MessageCount { get; set; }
        [JsonProperty("pendingMessageCount")] public long PendingMessageCount { get; set; }
    }

    public class InfobipIncomingResult
    {
        [JsonProperty("bulkId")] public string BulkId { get; set; }
        [JsonProperty("integrationType")] public string IntegrationType { get; set; }
        [JsonProperty("price")] public InfobipIncomingPrice Price { get; set; }
        [JsonProperty("status")] public InfobipIncomingInfoMessage Status { get; set; }
        [JsonProperty("error")] public InfobipIncomingInfoMessage Error { get; set; }
        [JsonProperty("messageId")] public string MessageId { get; set; }
        [JsonProperty("doneAt")] public DateTimeOffset? DoneAt { get; set; }
        [JsonProperty("messageCount")] public long MessageCount { get; set; }
        [JsonProperty("callbackData")] public string CallbackData { get; set; }

        /// <summary>
        /// When message was sent to subscriber in ISO8601 date time format
        /// </summary>
        [JsonProperty("sentAt")] public DateTimeOffset? SentAt { get; set; }
        [JsonProperty("to")] public string To { get; set; }
        [JsonProperty("from")] public string From { get; set; }
        [JsonProperty("channel")] public string Channel { get; set; }
        [JsonProperty("message")] public InfobipIncomingWhatsAppMessage Message { get; set; }
        [JsonProperty("contact")] public InfobipIncomingWhatsAppContact Contact { get; set; }
        [JsonProperty("receivedAt")] public DateTimeOffset? ReceivedAt { get; set; }
        

        /// <summary>
        /// when message was seen in ISO8601 date time format
        /// </summary>
        [JsonProperty("seenAt")] public DateTimeOffset? SeenAt { get; set; }

        /// <summary>
        /// Returns True if this message represents delivery report:
        /// https://dev-old.infobip.com/whatsapp-business-messaging/delivery-and-seen-reports
        /// </summary>
        /// <returns>True if this message represents delivery report</returns>
        public bool IsDeliveryReport()
        {
            return Status != null;
        }

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
        public bool IsMessage()
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

    public class InfobipIncomingInfoMessage
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("groupId")] public long GroupId { get; set; }
        [JsonProperty("groupName")] public string GroupName { get; set; }

        [JsonProperty("permanent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Permanent { get; set; }
    }

    public class InfobipIncomingPrice
    {
        [JsonProperty("pricePerMessage")] public long PricePerMessage { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; }
    }
    
    /// <summary>
    /// Types of reports sent to bot by Infobip server:
    /// https://dev-old.infobip.com/whatsapp-business-messaging/delivery-and-seen-reports
    /// </summary>
    public static class InfobipReportTypes
    {
        public const string SEEN = "SEEN";
        public const string DELIVERY = "DELIVERY";
    }
}