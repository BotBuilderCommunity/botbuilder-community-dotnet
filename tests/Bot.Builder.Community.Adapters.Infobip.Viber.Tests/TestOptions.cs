using System;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.Tests
{
    public class TestOptions
    {
        public static readonly string ApiKey = Guid.Empty.ToString();
        public const string ApiBaseUrl = "https://api.infobip.com";
        public const string ViberNumber = "447491163530";
        public const string AppSecret = "6250655368566D597133743677397A24";
        public const string ViberScenarioKey = "viber-scenario-key";

        public static InfobipViberAdapterOptions Get()
        {
            return new InfobipViberAdapterOptions(ApiKey, ApiBaseUrl, AppSecret, ViberNumber, ViberScenarioKey);
        }
    }
}