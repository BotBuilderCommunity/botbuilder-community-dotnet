namespace Bot.Builder.Community.Adapters.ACS.SMS.Core.Model
{
    public class DeliveryAttempt
    {
        public string Timestamp { get; set; }
        public int SegmentsSucceeded { get; set; }
        public int SegmentsFailed { get; set; }
    }
}