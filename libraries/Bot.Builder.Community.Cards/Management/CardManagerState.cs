using System;
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
        [JsonProperty("dataIdsByType")]
        public IDictionary<string, ISet<string>> DataIdsByType { get; } = new Dictionary<string, ISet<string>>();

        [JsonProperty("savedActivities")]
        public ISet<IMessageActivity> SavedActivities { get; } = new HashSet<IMessageActivity>();
    }
}