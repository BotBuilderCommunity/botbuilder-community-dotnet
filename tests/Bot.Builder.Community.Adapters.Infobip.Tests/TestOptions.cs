using System;
using Bot.Builder.Community.Adapters.Infobip;

namespace Bot.Builder.Community.Adapter.Infobip.Tests
{
    public class TestOptions
    {
        public static readonly string ApiKey = Guid.Empty.ToString();
        public const string ApiBaseUrl = "https://api.infobip.com";
        public const string WhatsAppNumber = "447491163530";
        public const string AppSecret = "6250655368566D597133743677397A24";
        public const string ScenarioKey = "scenario-key";

        public static InfobipAdapterOptions Get()
        {
            return new InfobipAdapterOptions(ApiKey, ApiBaseUrl, AppSecret, WhatsAppNumber, ScenarioKey);
        }
    }
}
