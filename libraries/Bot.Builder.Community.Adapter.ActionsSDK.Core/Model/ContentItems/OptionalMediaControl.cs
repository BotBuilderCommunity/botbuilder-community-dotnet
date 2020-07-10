using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OptionalMediaControl
    {
        OPTIONAL_MEDIA_CONTROLS_UNSPECIFIED,
        PAUSED,
        STOPPED
    }
}