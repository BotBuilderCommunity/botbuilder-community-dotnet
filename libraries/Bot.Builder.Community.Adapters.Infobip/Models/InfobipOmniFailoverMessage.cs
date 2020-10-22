using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Models
{
    public class InfobipOmniFailoverMessage
    {
        [JsonProperty("scenarioKey")] public string ScenarioKey { get; set; }
        [JsonProperty("destinations")] public InfobipDestination[] Destinations { get; set; }
        [JsonProperty("whatsApp")] public InfobipOmniWhatsAppMessage WhatsApp { get; set; }
        [JsonProperty("sms")] public InfobipOmniSmsMessage Sms { get; set; }
        [JsonProperty("callbackData")] public string CallbackData { get; set; }
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