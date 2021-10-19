using System;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class Message
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public DateTime CreateTime { get; set; }
        public string MessageId { get; set; }
    }
}