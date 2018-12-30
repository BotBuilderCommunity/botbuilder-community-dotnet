using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Storage.Elasticsearch
{
    /// <summary>
    /// Elasticsearch Storage Options.
    /// </summary>
    public class ElasticsearchStorageOptions
    {
        /// <summary>
        /// Gets or sets the Elasticsearch endpoint.
        /// </summary>
        /// <value>
        /// The Elasticsearch endpoint.
        /// </value>
        public Uri ElasticsearchEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the user name for Elasticsearch.
        /// </summary>
        /// <value>
        /// The user name for Elasticsearch.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password for Elasticsearch.
        /// </summary>
        /// <value>
        /// The password for Elasticsearch.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the index name for Elasticsearch instance.
        /// </summary>
        /// <value>
        /// The index name for Elasticsearch instance.
        /// </value>
        public string IndexName { get; set; }

        /// <summary>
        /// Gets or sets the index mapping depth limit for the Elasticsearch index.
        /// </summary>
        /// <value>
        /// The index mapping depth limit for the Elasticsearch index.
        /// </value>
        public int? IndexMappingDepthLimit { get; set; }
    }
}
