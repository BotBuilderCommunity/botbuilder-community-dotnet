using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class OutgoingEvent
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public GoogleEventType EventType { get; set; }

        public Representative Representative { get; set; } = new Representative()
        {
            DisplayName = "Bot",
            RepresentativeType = "BOT"
        };
    }
}
