using Azure.Core;

namespace Bot.Builder.Community.Adapters.ACS.SMS
{
    /// <summary>
    /// Options for the <see cref="AcsSmsAdapter"/>.
    /// </summary>
    public class AcsSmsAdapterOptions
    {
        public string AcsConnectionString { get; set; }

        public bool EnableDeliveryReports { get; set; } = true;

        public string AcsPhoneNumber { get; set; }

        public RetryOptions RetryOptions { get; set; }
    }
}
