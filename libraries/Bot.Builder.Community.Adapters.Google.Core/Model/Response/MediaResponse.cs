using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class MediaResponse : ResponseItem
    {
        [JsonProperty(PropertyName = "mediaResponse")]
        public MediaResponseContent Content { get; set; }
    }

    public class MediaResponseContent
    {
        public MediaType MediaType { get; set; }
        public MediaObject[] MediaObjects { get; set; }
    }

    public class MediaObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContentUrl { get; set; }
        public Image LargeImage { get; set; }
        public Image Icon { get; set; }

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MediaType
    {
        MEDIA_TYPE_UNSPECIFIED,
        AUDIO
    }
}