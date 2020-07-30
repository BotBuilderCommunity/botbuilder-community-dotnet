using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerState
    {
        /// <summary>
        /// Gets the dictionary that tracks data ID's.
        /// </summary>
        /// <value>
        /// The keys are data ID types and the values are the ID values.
        /// </value>
        [JsonProperty("dataIdsByScope")]
        public IDictionary<string, ISet<string>> DataIdsByScope { get; } = new Dictionary<string, ISet<string>>();

        [JsonProperty("savedActivities")]
        public ISet<Activity> SavedActivities { get; } = new HashSet<Activity>();
    }
}