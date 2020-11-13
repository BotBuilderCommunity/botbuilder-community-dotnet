using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Sms.Models
{
    public class InfobipOmniSmsFailoverMessage: InfobipOmniFailoverMessageBase
    {
        [JsonProperty("sms")] public InfobipOmniSmsMessage Sms { get; set; }


        public override string ToJson()
        {
            return JsonConvert.SerializeObject(this, InfobipSerialize.Settings);
        }
    }

    public class InfobipOmniSmsMessage : InfobipOmniSmsMessageOptions
    {
        [JsonProperty("text")] public string Text { get; set; }

        public void SetOptions(InfobipOmniSmsMessageOptions options)
        {
            ValidityPeriod = options.ValidityPeriod;
            ValidityPeriodTimeUnit = options.ValidityPeriodTimeUnit;
            Transliteration = options.Transliteration;
            Language = options.Language;
        }
    }

    public class InfobipOmniSmsLanguage
    {
        [JsonProperty("languageCode")] public string LanguageCode { get; set; }
    }

    public class InfobipOmniSmsMessageOptions
    {
        [JsonProperty("validityPeriod")] public int ValidityPeriod { get; set; }
        [JsonProperty("validityPeriodTimeUnit")] public string ValidityPeriodTimeUnit { get; set; }
        [JsonProperty("transliteration")] public string Transliteration { get; set; }
        [JsonProperty("language")] public InfobipOmniSmsLanguage Language { get; set; }
    }
}