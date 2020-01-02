using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerState
    {
        [JsonProperty("trackedIdsByType")]
        public IDictionary<IdType, ICollection<string>> TrackedIdsByType { get; } = new Dictionary<IdType, ICollection<string>>();

        [JsonProperty("activitiesById")]
        public IDictionary<string, ISet<Activity>> ActivitiesById { get; } = new Dictionary<string, ISet<Activity>>();
    }
}