using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Core.Models
{
    public abstract class InfobipOmniFailoverMessageBase
    {
        [JsonProperty("scenarioKey")] public string ScenarioKey { get; set; }
        [JsonProperty("destinations")] public InfobipDestination[] Destinations { get; set; }
        [JsonProperty("callbackData")] public string CallbackData { get; set; }

        public abstract string ToJson();
    }

    public class InfobipDestination
    {
        [JsonProperty("to")] public InfobipTo To { get; set; }
    }

    public class InfobipTo
    {
        [JsonProperty("phoneNumber")] public string PhoneNumber { get; set; }
    }
}
