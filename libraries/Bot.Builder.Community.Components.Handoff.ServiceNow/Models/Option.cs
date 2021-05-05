using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Components.Handoff.ServiceNow.Models
{
    public class Option
    {
        public string label { get; set; }
        public string description { get; set; }
        public string attachment { get; set; }
        public string value { get; set; }
        public bool enabled { get; set; }
    }
}
