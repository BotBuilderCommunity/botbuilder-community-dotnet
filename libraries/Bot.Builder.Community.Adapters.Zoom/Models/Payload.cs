namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class Payload
    {
        public string AccountId { get; set; }
        public string ChannelName { get; set; }
        public string RobotJid { get; set; }
        public long Timestamp { get; set; }
        public string ToJid { get; set; }
        public string UserId { get; set; }
        public string UserJid { get; set; }
        public string UserName { get; set; }
    }

    public class BotNotificationPayload : Payload
    {
        public string Cmd { get; set; }
        public string Name { get; set; }
    }

    public class InteractiveMessageActionsPayload : Payload
    {
        public ZoomAction ZoomActionItem { get; set; }

        public ChatResponseContent Original { get; set; }
    }
}