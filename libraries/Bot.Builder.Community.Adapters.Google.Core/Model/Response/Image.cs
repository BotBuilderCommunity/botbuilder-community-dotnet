using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class Image
    {
        public string Url { get; set; }
        public string AccessibilityText { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageDisplayOptions
    {
        DEFAULT,
        WHITE,
        CROPPED
    }
}