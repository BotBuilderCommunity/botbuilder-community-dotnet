using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Slack.Model
{
    public class ModalView
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "state")]
        public ModelViewState State { get; set; }

        [JsonProperty(PropertyName = "callback_id")]
        public string ModalIndentifier { get; set; }

        [JsonProperty(PropertyName = "private_metadata")]
        public string PrivateMetadata { get; set; }
    }
}
