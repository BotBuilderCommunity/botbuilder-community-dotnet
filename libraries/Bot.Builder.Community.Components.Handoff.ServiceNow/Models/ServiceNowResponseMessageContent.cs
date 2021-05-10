namespace Bot.Builder.Community.Components.Handoff.ServiceNow.Models
{
    public class ServiceNowResponseMessageContent
    {
        public string text { get; set; }
        public bool typed { get; set; }
        public string clientMessageId { get; set; }
        public Attachment attachment { get; set; }
    }
}
