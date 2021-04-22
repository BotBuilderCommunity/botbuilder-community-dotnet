using Azure.Core;
using System.ComponentModel.DataAnnotations;

namespace Bot.Builder.Community.Adapters.ACS.SMS
{
    /// <summary>
    /// Options for the <see cref="AcsSmsAdapter"/>.
    /// </summary>
    public class AcsSmsAdapterOptions
    {
        [Required]
        public string AcsConnectionString { get; set; }

        public bool EnableDeliveryReports { get; set; } = true;

        [Required]
        public string AcsPhoneNumber { get; set; }

        public RetryOptions RetryOptions { get; set; }
    }
}
