using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.Models
{
    //https://www.infobip.com/docs/api#programmable-communications/omni-failover/send-omni-failover-message
    public class InfobipViberOmniFailoverMessage: InfobipOmniFailoverMessageBase
    {
        [JsonProperty("viber")] public InfobipOmniViberMessage Viber { get; set; }


        public override string ToJson()
        {
            return JsonConvert.SerializeObject(this, InfobipSerialize.Settings);
        }
    }

    public class InfobipOmniViberMessage
    {
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("validityPeriod")] public int ValidityPeriod { get; set; }
        [JsonProperty("validityPeriodTimeUnit")] public string ValidityPeriodTimeUnit { get; set; }
        [JsonProperty("imageURL")] public string ImageUrl { get; set; }
        [JsonProperty("buttonText")] public string ButtonText { get; set; }
        [JsonProperty("buttonURL")] public string ButtonUrl { get; set; }
        [JsonProperty("trackingData")] public string TrackingData { get; set; }
        [JsonProperty("isPromotional")] public bool IsPromotional { get; set; }
    }
}
