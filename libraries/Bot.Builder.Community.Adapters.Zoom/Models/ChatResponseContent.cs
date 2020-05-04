using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class ChatResponseContent
    {
        public Head Head { get; set; }
        public List<BodyItem> Body { get; set; }
    }
}