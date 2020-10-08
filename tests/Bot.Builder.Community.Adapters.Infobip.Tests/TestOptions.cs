using System;

namespace Bot.Builder.Community.Adapters.Infobip.Tests
{
    public class TestOptions
    {
        public static readonly string ApiKey = Guid.Empty.ToString();
        public const string ApiBaseUrl = "https://api.infobip.com";
        public const string WhatsAppNumber = "447491163530";
        public const string AppSecret = "6250655368566D597133743677397A24";
        public const string WhatsAppScenarioKey = "whatsapp-scenario-key";
        public const string SmsScenarioKey = "sms-scenario-key";
        public const string SmsNumber = "123456789";

        public static InfobipAdapterOptions Get()
        {
            return new InfobipAdapterOptions(ApiKey, ApiBaseUrl, AppSecret, WhatsAppNumber, SmsNumber, WhatsAppScenarioKey, SmsScenarioKey);
        }
    }
}