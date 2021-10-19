using Bot.Builder.Community.Components.Handoff.ServiceNow.Converters;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Handoff.ServiceNow.Models
{
    public class Body
    {
        public string uiType { get; set; }
        public string group { get; set; }
        [JsonConverter(typeof(ObjectOrStringConverter<BodyValue>))]
        public object value { get; set; }
        public string label { get; set; }
        public bool nluTextEnabled { get; set; }
        public string promptMsg { get; set; }
        public Option[] options { get; set; }
        public string header { get; set; }
        public Value[] values { get; set; }
        public string templateName { get; set; }
        public object data { get; set; }
    }

    public class BodyValue
    {
        public string action { get; set; }
    }
}
