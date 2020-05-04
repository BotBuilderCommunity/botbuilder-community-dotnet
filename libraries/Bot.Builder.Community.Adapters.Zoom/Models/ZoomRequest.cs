using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Zoom.Models
{
    public class ZoomRequest
    {
        public string Event { get; set; }
        public JObject Payload { get; set; }
    }
}
