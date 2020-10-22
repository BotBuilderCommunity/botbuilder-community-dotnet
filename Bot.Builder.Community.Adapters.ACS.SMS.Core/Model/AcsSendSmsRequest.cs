using Azure.Communication;
using Azure.Communication.Sms;

namespace Bot.Builder.Community.Adapters.ACS.SMS.Core.Model
{
    public class AcsSendSmsRequest
    {
        public PhoneNumber From { get; set; }
        
        public PhoneNumber To { get; set; }

        public string Message { get; set; }

        public SendSmsOptions Options { get; set; }

        public bool EnableDeliveryReport { get; set; } = false;
    }
}
