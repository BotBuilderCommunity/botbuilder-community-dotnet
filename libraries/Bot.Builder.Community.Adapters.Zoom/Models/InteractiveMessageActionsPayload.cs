namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class InteractiveMessageActionsPayload : InteractiveMessagePayload
    {
        public ZoomAction ActionItem { get; set; }
    }
}