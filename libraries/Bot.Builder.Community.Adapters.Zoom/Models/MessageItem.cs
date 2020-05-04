namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class MessageItem : BodyItem
    {
        public string Type => "message";

        public string Text { get; set; }

        public Style Style { get; set; }
    }
}