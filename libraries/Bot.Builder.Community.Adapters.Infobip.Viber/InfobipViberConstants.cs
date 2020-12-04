namespace Bot.Builder.Community.Adapters.Infobip.Viber
{
    public class InfobipViberConstants
    {
        public const string ChannelName = "infobip-viber";
    }

    public static class InfobipViberMessageContentTypes
    {
        public const string Message = "application/vhd.infobip.viber-message";
    }

    public static class InfobipViberOptions
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
    }
}
