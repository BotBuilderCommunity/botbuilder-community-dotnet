namespace Bot.Builder.Community.Cards
{
    public static class CardConstants
    {
        public const string AdaptiveCardContentType = "application/vnd.microsoft.card.adaptive";
        public const string ActionSubmit = "Action.Submit";

        // Adaptive Card property names
        public const string KeyActions = "actions";
        public const string KeyBody = "body";
        public const string KeyData = "data";
        public const string KeyId = "id";
        public const string KeyType = "type";
        public const string KeyValue = "value";

        // Channel data property names
        public const string KeyCallbackQuery = "callback_query";
        public const string KeyLinePostback = "postback";
        public const string KeyMessageBack = "messageBack";
        public const string KeyMetadata = "metadata";
        public const string KeyPostBack = "postBack";
        public const string KeyPayload = "Payload";
        public const string KeyText = "text";

        // Entity types
        public const string TypeIntent = "Intent";

        internal const string PackageId = "botBuilderCommunityCards";
    }
}