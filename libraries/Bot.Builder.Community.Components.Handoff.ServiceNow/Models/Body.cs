namespace Bot.Builder.Community.Components.Handoff.ServiceNow.Models
{
    public class Body
    {
        public string uiType { get; set; }
        public string group { get; set; }
        public string value { get; set; }
        public string label { get; set; }
        public bool nluTextEnabled { get; set; }
        public string promptMsg { get; set; }
        public Option[] options { get; set; }
        public string header { get; set; }
        public Value[] values { get; set; }
    }
}
