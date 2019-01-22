using Microsoft.Bot.Schema;
using Nest;
using System;

namespace Bot.Builder.Community.TranscriptStore.Elasticsearch
{
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class DocumentItem
    {
        /// <summary>
        /// Gets or sets the object id.
        /// </summary>
        /// <value>
        /// The object id.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the channel id.
        /// </summary>
        /// <value>
        /// The channel id.
        /// </value>
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the conversation id.
        /// </summary>
        /// <value>
        /// The conversation id.
        /// </value>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the persisted activity.
        /// </summary>
        /// <value>
        /// The persisted activity.
        /// </value>
        public Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets the current timestamp.
        /// </summary>
        /// <value>
        /// The current timestamp.
        /// </value>
        public DateTimeOffset? Timestamp { get; set; }
    }
}
