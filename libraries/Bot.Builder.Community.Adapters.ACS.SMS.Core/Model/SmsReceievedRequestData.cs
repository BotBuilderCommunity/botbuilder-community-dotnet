namespace Bot.Builder.Community.Adapters.ACS.SMS.Core.Model
{
    public class SmsReceivedRequestData
    {
        public string From { get; set; }
        public string To { get; set; }
        public string MessageId { get; set; }
        public string Message { get; set; }
        public string ReceivedTimestamp { get; set; }
    }
}