namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Helper
{
    public static class ActivityHelper
    {
        public static double ExpireAfterSeconds { get; set; } = 0;

        public static int SleepInMilliseconds { get; set; } = 0;

        public static double ReminderSeconds { get; set; } = 0;

        public static string SessionExpireTrigger = "SessionExpireConversation";

        public static string ReminderTrigger = "ReminderConversation";

        public static bool IsEnable;
    }
}