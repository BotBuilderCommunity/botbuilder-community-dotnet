using System.Collections.Generic;

namespace Bot.Builder.Community.Components.Handoff.LivePerson.Models
{
    public class EventContent
    {
        public string Type { get; set; }
        public string Tag { get; set; }
        public List<ContentElement> Elements { get; set; }
    }
}