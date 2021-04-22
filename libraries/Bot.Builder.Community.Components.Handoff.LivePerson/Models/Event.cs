namespace Bot.Builder.Community.Components.Handoff.LivePerson.Models
{
    public class Event
    {
        public string type { get; set; }
        public string chatState { get; set; }
        public string message { get; set; }
        public string contentType { get; set; }
        public EventContent Content { get; set; }
    }
}