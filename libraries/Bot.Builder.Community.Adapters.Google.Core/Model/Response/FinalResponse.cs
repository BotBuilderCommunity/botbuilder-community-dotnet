using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class FinalResponse
    {
        [JsonProperty(PropertyName = "richResponse")]
        public RichResponse RichResponse { get; set; }
    }
}