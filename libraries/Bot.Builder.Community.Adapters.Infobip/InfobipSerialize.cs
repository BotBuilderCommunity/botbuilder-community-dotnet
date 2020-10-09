using Bot.Builder.Community.Adapters.Infobip.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Bot.Builder.Community.Adapters.Infobip
{
    public static class InfobipSerialize
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
        };

        public static string ToInfobipOmniFailoverOmniMessageJson(this InfobipOmniFailoverMessage self) =>
            JsonConvert.SerializeObject(self, Settings);

        public static InfobipOmniResponse FromInfobipOmniResponseJson(this string json) =>
            JsonConvert.DeserializeObject<InfobipOmniResponse>(json, Settings);

        public static InfobipIncomingMessage FromInfobipIncomingMessageJson(this string json) =>
            JsonConvert.DeserializeObject<InfobipIncomingMessage>(json, Settings);

        public static string ToInfobipCallbackDataJson(this JObject self) =>
            JsonConvert.SerializeObject(self, Settings);

        public static Dictionary<string, string> ToDictionary(this JObject self) =>
            JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(self, Settings));

        public static InfobipOmniSmsMessageOptions ToInfobipOmniSmsMessageOptions(this JObject self) =>
            JsonConvert.DeserializeObject<InfobipOmniSmsMessageOptions>(JsonConvert.SerializeObject(self, Settings));
    }
}