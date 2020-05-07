using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class SimpleResponse : ResponseItem
    {
        [JsonProperty(PropertyName = "simpleResponse")]
        public SimpleResponseContent Content { get; set; }
    }

    public class SimpleResponseContent
    {
        public string TextToSpeech { get; set; }
        public string Ssml { get; set; }
        public string DisplayText { get; set; }
    }
}