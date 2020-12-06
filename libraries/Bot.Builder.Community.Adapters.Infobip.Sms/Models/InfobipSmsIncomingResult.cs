using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Sms.Models
{
    public class InfobipSmsIncomingResult: InfobipIncomingResultBase
    {
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("cleanText")] public string CleanText { get; set; }
        [JsonProperty("smsCount")] public long SmsCount { get; set; }

        /// <summary>
        /// Returns True if this message represents SMS message sent by subscriber to bot:
        /// https://dev-old.infobip.com/receive-sms/forward-method
        /// </summary>
        /// <returns>True if this message represents SMS message sent by subscriber to bot</returns>
        public bool IsSmsMessage()
        {
            return SmsCount > 0;
        }
    }
}