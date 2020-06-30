using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents
{
    public class IntentInputValueData
    {
        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }
    }
}