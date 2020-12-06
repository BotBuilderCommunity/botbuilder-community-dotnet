using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MediaType
    {
        MEDIA_TYPE_UNSPECIFIED,
        AUDIO,
        MEDIA_STATUS_ACK
    }
}