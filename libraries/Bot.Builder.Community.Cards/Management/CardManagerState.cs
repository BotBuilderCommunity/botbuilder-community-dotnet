using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerState
    {
        [JsonProperty("payloadIdsByType")]
        public IDictionary<PayloadIdType, ISet<string>> PayloadIdsByType { get; } = new Dictionary<PayloadIdType, ISet<string>>();

        [JsonProperty("activityIdsByPayloadId")]
        public IDictionary<string, ISet<string>> ActivityIdsByPayloadId { get; } = new Dictionary<string, ISet<string>>(StringComparer.OrdinalIgnoreCase);

        [JsonProperty("activitiesById")]
        public IDictionary<string, Activity> ActivitiesById { get; } = new Dictionary<string, Activity>(StringComparer.OrdinalIgnoreCase);
    }
}