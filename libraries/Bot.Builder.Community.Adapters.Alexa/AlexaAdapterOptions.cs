namespace Bot.Builder.Community.Adapters.Alexa
{
    public class AlexaAdapterOptions
    {
        public bool TryConvertFirstActivityAttachmentToAlexaCard { get; set; } = false;

        public bool ValidateIncomingAlexaRequests { get; set; } = true;

        public bool ShouldEndSessionByDefault { get; set; } = true;

        public bool TryConvertSuggestedActionsToText { get; set; } = false;

        public bool TryConcatenateTextFromMultipleActivities { get; set; } = false;

    }
}
