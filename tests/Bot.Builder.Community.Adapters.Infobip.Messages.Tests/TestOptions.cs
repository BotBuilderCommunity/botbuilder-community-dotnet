using System;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Tests
{
    public class TestOptions
    {
        public static readonly string ApiKey = Guid.Empty.ToString();
        public const string ApiBaseUrl = "https://api.infobip.com";
        public const string AppSecret = "6250655368566D597133743677397A24";
        public const string MessagesApiBaseUrl = "https://api.infobip.com";

        public static InfobipMessagesAdapterOptions Get()
        {
            return new InfobipMessagesAdapterOptions(ApiKey, MessagesApiBaseUrl, AppSecret);
        }
    }
}