namespace Bot.Builder.Community.Components.Handoff.LivePerson.Models
{
    public class Change
    {
        public int sequence { get; set; }
        public string originatorId { get; set; }
        public Originatormetadata originatorMetadata { get; set; }
        public long serverTimestamp { get; set; }
        public Event @event { get; set; }
        public string conversationId { get; set; }
        public string dialogId { get; set; }
        public Result result { get; set; }
    }
}