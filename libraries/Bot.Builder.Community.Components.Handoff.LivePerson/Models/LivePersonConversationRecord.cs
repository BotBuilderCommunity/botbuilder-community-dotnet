namespace Bot.Builder.Community.Components.Handoff.LivePerson.Models
{
    public class LivePersonConversationRecord
    {
        public string ConversationId { get; set; }
        public string MessageDomain { get; set; }
        public string AppJWT { get; set; }
        public string ConsumerJWS { get; set; }
        public bool IsClosed { get; set; }
        public bool IsAcknowledged { get; set; }
    }
}