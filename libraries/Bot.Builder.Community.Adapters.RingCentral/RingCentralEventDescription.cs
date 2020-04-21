namespace Bot.Builder.Community.Adapters.RingCentral
{
    public static class RingCentralEventDescription
    {
        public const string MessageCreate = "messages.create";
        public const string ImplementationInfo = "implementation.info";
        public const string HandoffNotification = "handoff_notification";

        public const string InterventionOpened = "intervention.opened";
        public const string InterventionCanceled = "intervention.canceled";
        public const string InterventionClosed = "intervention.closed";
        public const string InterventionDeferred = "intervention.deferred";
        public const string InterventionReactivated = "intervention.reactivated";
        public const string InterventionReopend = "intervention.reopened";

        public const string ContentImported = "content.imported";
        public const string MessageList = "messages.list";
        public const string PrivateMessagesList = "private_messages.list";
        public const string ThreadsList = "threads.list";
        public const string PrivateMessagesShow = "private_messages.show";
        public const string ThreadsShow = "threads.show";
    }
}
