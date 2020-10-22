using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.ACS.SMS.Core.Model
{
    public class SmsDeliveryReportReceivedRequestData
    {
        public string MessageId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string DeliveryStatus { get; set; }
        public string DeliveryStatusDetails { get; set; }
        public List<DeliveryAttempt> DeliveryAttempts { get; set; }
        public string ReceivedTimestamp { get; set; }
    }
}