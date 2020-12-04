using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.Models
{
    public class InfobipViberIncomingResult: InfobipIncomingResultBase
    {
        public InfobipViberIncomingMessage Message { get; set; }

        /// <summary>
        /// Returns True if this message represents message sent by subscriber to bot:
        /// https://dev-old.infobip.com/omni-channel/viber-example#4-receive-incoming-viber-message
        /// </summary>
        /// <returns>True if this message represents message sent by subscriber to bot</returns>
        public bool IsViberMessage()
        {
            const string viberChannelName = "VIBER";
            return IntegrationType == viberChannelName && Message != null;
        }
    }

    public class InfobipViberIncomingMessage
    {
        [JsonProperty("type")] public string Type { get; set; } = "TEXT";
        [JsonProperty("text")] public string Text { get; set; }
    }
}
