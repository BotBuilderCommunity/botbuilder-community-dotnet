using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class BasicCard : ResponseItem
    {
        [JsonProperty(PropertyName = "basicCard")]
        public BasicCardContent Content { get; set; }
    }

    public class BasicCardContent
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string FormattedText { get; set; }
        public Image Image { get; set; }
        public Button[] Buttons { get; set; }
        public ImageDisplayOptions? ImageDisplayOptions { get; set; }
    }
}