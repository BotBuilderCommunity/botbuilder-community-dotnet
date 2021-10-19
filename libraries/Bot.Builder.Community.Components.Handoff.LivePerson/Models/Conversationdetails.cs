namespace Bot.Builder.Community.Components.Handoff.LivePerson.Models
{
    public class Conversationdetails
    {
        public string skillId { get; set; }
        public Participant[] participants { get; set; }
        public Dialog[] dialogs { get; set; }
        public string brandId { get; set; }
        public string state { get; set; }
        public string stage { get; set; }
        public string closeReason { get; set; }
        public long startTs { get; set; }
        public long endTs { get; set; }
        public long metaDataLastUpdateTs { get; set; }
        public Ttr ttr { get; set; }
        public Conversationhandlerdetails conversationHandlerDetails { get; set; }
    }
}