using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardManagerState
    {
        private IDictionary<PayloadIdType, ISet<string>> _payloadIdsByType = new Dictionary<PayloadIdType, ISet<string>>();

        /// <summary>
        /// Gets or sets the dictionary that tracks payload ID's.
        /// </summary>
        /// <value>
        /// To allow for validation of dictionary keys, this getter only returns a copy of the backing field.
        /// Therefore, you must set the property explicitly after making changes.
        /// </value>
        [JsonProperty("payloadIdsByType")]
        public IDictionary<PayloadIdType, ISet<string>> PayloadIdsByType
        {
            get => new Dictionary<PayloadIdType, ISet<string>>(_payloadIdsByType);

            set
            {
                if (value != null)
                {
                    value.Keys.CheckIdTypes();
                    _payloadIdsByType = value; 
                }
            }
        }

        /// <summary>
        /// Gets the dictionary that saves activity ID's.
        /// </summary>
        /// <value>
        /// For any payload ID that's not a batch ID,
        /// the hash set should only contain one activity ID.
        /// </value>
        [JsonProperty("activityIdsByPayloadId")]
        public IDictionary<string, ISet<string>> ActivityIdsByPayloadId { get; } = new Dictionary<string, ISet<string>>();

        [JsonProperty("activityById")]
        public IDictionary<string, Activity> ActivityById { get; } = new Dictionary<string, Activity>();
    }
}