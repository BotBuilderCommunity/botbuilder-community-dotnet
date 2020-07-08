using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TypeOverrideMode
    {
        TYPE_REPLACE,
        TYPE_MERGE
    }
}