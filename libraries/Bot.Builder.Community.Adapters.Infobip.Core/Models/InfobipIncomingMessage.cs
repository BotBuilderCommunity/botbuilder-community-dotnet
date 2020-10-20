using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Infobip.Core.Models
{
    public class InfobipIncomingMessage<T> where T : InfobipIncomingResultBase
    {
        [JsonProperty("results")] public List<T> Results { get; set; }
        [JsonProperty("messageCount")] public long MessageCount { get; set; }
        [JsonProperty("pendingMessageCount")] public long PendingMessageCount { get; set; }
    }
}
