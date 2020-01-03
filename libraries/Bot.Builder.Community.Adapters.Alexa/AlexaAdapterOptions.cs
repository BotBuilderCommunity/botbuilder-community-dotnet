namespace Bot.Builder.Community.Adapters.Alexa
{
    public class AlexaAdapterOptions
    {
        public bool TryConvertFirstActivityAttachmentToAlexaCard { get; set; } = false;

        public bool ValidateIncomingAlexaRequests { get; set; } = true;

        public bool ShouldEndSessionByDefault { get; set; } = true;

        public MultipleOutgoingActivityPolicies MultipleOutgoingActivityPolicy { get; set; } = MultipleOutgoingActivityPolicies.TakeLastActivity;
    }

    public enum MultipleOutgoingActivityPolicies
    {
        TakeFirstActivity,
        TakeLastActivity,
        ConcatenateTextSpeakPropertiesFromAllActivities
    }
}
