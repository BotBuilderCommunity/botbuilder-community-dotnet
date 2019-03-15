using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Google
{
    public class GoogleResponseBody
    {
        public ResponsePayload Payload { get; set; }
    }

    public class ResponsePayload
    {
        public PayloadContent Google { get; set; }
    }

    public class PayloadContent
    {
        public bool ExpectUserResponse { get; set; }
        public RichResponse RichResponse { get; set; }
        public ISystemIntent SystemIntent { get; set; }
    }

    public class ISystemIntent
    {
        public string Intent { get; set; }
    }

    public class RichResponse
    {
        public Item[] Items { get; set; }
        public Suggestion[] Suggestions { get; set; }
        public LinkOutSuggestion LinkOutSuggestion { get; set; }
    }

    public class LinkOutSuggestion
    {
        public string DestinationName { get; set; }
        public string Url { get; set; }
        public OpenUrlAction OpenUrlAction { get; set; }
    }

    public class Suggestion
    {
        public string Title { get; set; }
    }

    public class Item
    {
    }

    public class SimpleResponse : Item
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

    public class GoogleBasicCard : Item
    {
        [JsonProperty(PropertyName = "basicCard")]
        public GoogleBasicCardContent Content { get; set; }
    }

    public class GoogleBasicCardContent
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string FormattedText { get; set; }
        public Image Image { get; set; }
        public Button[] Buttons { get; set; }
        public ImageDisplayOptions? Display { get; set; }
    }

    public class MediaResponseContent
    {
        public MediaType MediaType { get; set; }
        public MediaObject[] MediaObjects { get; set; }
    }

    public class MediaResponse : Item
    {
        [JsonProperty(PropertyName = "mediaResponse")]
        public MediaResponseContent Content { get; set; }
    }

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

    public class Button
    {
        public string Title { get; set; }
        public OpenUrlAction OpenUrlAction { get; set; }
    }

    public class OpenUrlAction
    {
        public string Url { get; set; }
        public AndroidApp AndroidApp { get; set; }
        public UrlTypeHint UrlTypeHint { get; set; }
    }

    public class AndroidApp
    {
        public string PackageName { get; set; }
        public VersionFilter[] Versions { get; set; }
    }

    public class VersionFilter
    {
        public double MinVersion { get; set; }
        public double MaxVersion { get; set; }
    }

    public enum UrlTypeHint
    {
        URL_TYPE_HINT_UNSPECIFIED,
        AMP_CONTENT
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
