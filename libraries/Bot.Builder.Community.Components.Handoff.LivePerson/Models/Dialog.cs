namespace Bot.Builder.Community.Components.Handoff.LivePerson.Models
{
    public class Dialog
    {
        public string dialogId { get; set; }
        public Participantsdetail[] participantsDetails { get; set; }
        public string dialogType { get; set; }
        public string channelType { get; set; }
        public Metadata metaData { get; set; }
        public string state { get; set; }
        public long creationTs { get; set; }
        public long endTs { get; set; }
        public long metaDataLastUpdateTs { get; set; }
        public string closedBy { get; set; }
        public string closedCause { get; set; }
    }
}