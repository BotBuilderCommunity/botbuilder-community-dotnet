namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class InteractiveMessagePayload : Payload
    {
        public string MessageId { get; set; }

        public ChatResponseContent Original { get; set; }
    }
}