using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerState
    {
        /// <summary>
        /// Gets the dictionary that tracks payload ID's.
        /// </summary>
        /// <value>
        /// The keys are payload ID types and the values are the ID values.
        /// </value>
        [JsonProperty("payloadIdsByType")]
        public IDictionary<string, ISet<string>> PayloadIdsByType { get; } = new Dictionary<string, ISet<string>>();

        [JsonProperty("activityById")]
        public ISet<IMessageActivity> SavedActivities { get; } = new HashSet<IMessageActivity>();
    }
}