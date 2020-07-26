using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageFill
    {
        UNSPECIFIED,
        GREY,
        WHITE,
        CROPPED
    }
}