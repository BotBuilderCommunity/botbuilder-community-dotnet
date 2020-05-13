namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class BotNotificationPayload : Payload
    {
        public string Cmd { get; set; }
        public string Name { get; set; }
    }
}