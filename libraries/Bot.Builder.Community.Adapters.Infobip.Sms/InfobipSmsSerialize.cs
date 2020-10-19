using Bot.Builder.Community.Adapters.Infobip.Sms.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Infobip.Sms
{
    public static class InfobipSmsSerialize
    {
        public static InfobipOmniSmsMessageOptions ToInfobipOmniSmsMessageOptions(this JObject self) =>
            JsonConvert.DeserializeObject<InfobipOmniSmsMessageOptions>(JsonConvert.SerializeObject(self, Core.InfobipSerialize.Settings));
    }
}