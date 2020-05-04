namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class ZoomField
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Editable { get; set; } = true;
        public Style Style { get; set; }
    }
}