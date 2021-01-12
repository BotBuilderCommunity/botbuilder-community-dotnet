namespace Bot.Builder.Community.Adapters.ACS.SMS.Core
{
    public class AcsSmsRequestMapperOptions
    {
        public string ChannelId { get; set; } = "ACS_SMS";
        public string ServiceUrl { get; set; }
        public string AcsPhoneNumber { get; set; }
        public bool EnableDeliveryReports { get; set; } = true;
    }
}
