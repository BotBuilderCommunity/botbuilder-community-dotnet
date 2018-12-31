using Newtonsoft.Json.Linq;
using System;

namespace Bot.Builder.Community.Storage.Elasticsearch
{
    public class DocumentItem
    {
        /// <summary>
        /// Gets or sets the sanitized Id/Key as Object Id.
        /// </summary>
        /// <value>
        /// The sanitized Id/Key as Object Id.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the un-sanitized Id/Key.
        /// </summary>
        /// <value>
        /// The un-sanitized Id/Key.
        /// </value>
        public string RealId { get; set; }

        /// <summary>
        /// Gets or sets the persisted object's state.
        /// </summary>
        /// <value>
        /// The persisted object's state.
        /// </value>
        public JObject Document { get; set; }

        /// <summary>
        /// Gets or sets the current timestamp.
        /// </summary>
        /// <value>
        /// The current timestamp.
        /// </value>
        public DateTime Timestamp { get; set; }
    }
}
