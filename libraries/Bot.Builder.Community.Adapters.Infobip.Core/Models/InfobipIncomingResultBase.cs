using Newtonsoft.Json;
using System;

namespace Bot.Builder.Community.Adapters.Infobip.Core.Models
{
    public abstract class InfobipIncomingResultBase
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
        [JsonProperty("receivedAt")] public DateTimeOffset? ReceivedAt { get; set; }
        [JsonProperty("keyword")] public string Keyword { get; set; }

        /// <summary>
        /// Returns True if this message represents delivery report:
        /// https://dev-old.infobip.com/whatsapp-business-messaging/delivery-and-seen-reports
        /// </summary>
        /// <returns>True if this message represents delivery report</returns>
        public bool IsDeliveryReport()
        {
            return Status != null;
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
