using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models
{
    public class InfobipOmniFailoverMessage: InfobipOmniFailoverMessageBase
    {
        [JsonProperty("whatsApp")] public InfobipOmniWhatsAppMessage WhatsApp { get; set; }

        public override string ToJson()
        {
            return JsonConvert.SerializeObject(this, InfobipSerialize.Settings);
        }
    }
}