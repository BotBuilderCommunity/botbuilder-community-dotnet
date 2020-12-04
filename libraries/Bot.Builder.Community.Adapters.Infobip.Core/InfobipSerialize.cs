using System.Collections.Generic;
using System.Globalization;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Infobip.Core
{
    public static class InfobipSerialize
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
        };

        public static InfobipOmniResponse FromInfobipOmniResponseJson(this string json) =>
            JsonConvert.DeserializeObject<InfobipOmniResponse>(json, Settings);

        public static InfobipIncomingMessage<T> FromInfobipIncomingMessageJson<T>(this string json) where T :InfobipIncomingResultBase =>
            JsonConvert.DeserializeObject<InfobipIncomingMessage<T>>(json, Settings);

        public static string ToInfobipCallbackDataJson(this JObject self) =>
            JsonConvert.SerializeObject(self, Settings);

        public static Dictionary<string, string> ToDictionary(this JObject self) =>
            JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(self, Settings));

        public static InfobipOmniSmsMessageOptions ToInfobipOmniSmsMessageOptions(this JObject self) =>
            JsonConvert.DeserializeObject<InfobipOmniSmsMessageOptions>(JsonConvert.SerializeObject(self, Settings));
    }
}