namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class InteractiveMessageFieldsEditablePayload : InteractiveMessagePayload
    {
        public FieldEditItem EditedField { get; set; }
    }

    public class FieldEditItem
    {
        public string CurrentValue { get; set; }
        public string Key { get; set; }
        public string NewValue { get; set; }
    }
}