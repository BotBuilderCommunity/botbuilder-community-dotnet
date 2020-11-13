namespace Bot.Builder.Community.Adapters.Infobip.Sms
{
    public class InfobipSmsConstants
    {
        public const string ChannelName = "infobip-sms";
    }

    public class InfobipSmsEntityType
    {
        public const string SmsMessageOptions = "application/vhd.infobip.sms-message-options";
    }

    public static class InfobipSmsOptions
    {
        public class ValidityPeriodTimeUnitTypes
        {
            public const string Nanoseconds = "NANOSECONDS";
            public const string Microseconds = "MICROSECONDS";
            public const string Miliseconds = "MILLISECONDS";
            public const string Seconds = "SECONDS";
            public const string Minutes = "MINUTES";
            public const string Hours = "HOURS";
            public const string Days = "DAYS";
        }

        public class TransliterationTypes
        {
            public const string None = "NONE";
            public const string All = "ALL";
            public const string Baltic = "BALTIC";
            public const string CentralEuropean = "CENTRAL_EUROPEAN";
            public const string Colombian = "COLOMBIAN";
            public const string Cyrillic = "CYRILLIC";
            public const string Greek = "GREEK";
            public const string NonUnicode = "NON_UNICODE";
            public const string Portuguese = "PORTUGUESE";
            public const string SerbianCyrillic = "SERBIAN_CYRILLIC";
            public const string Turkish = "TURKISH";
        }

        public class LanguageCode
        {
            public const string None = "NONE";
            public const string Tr = "TR";
            public const string Es = "ES";
            public const string Pt = "PT";
            public const string Autodetect = "AUTODETECT";
        }
    }
}