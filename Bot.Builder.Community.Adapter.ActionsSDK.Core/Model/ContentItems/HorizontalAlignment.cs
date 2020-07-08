using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HorizontalAlignment
    {
        UNSPECIFIED,
        LEADING,
        CENTER,
        TRAILING
    }
}