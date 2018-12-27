using Elasticsearch.Net;
using Microsoft.Bot.Builder;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Storage.Elasticsearch
{
    /// <summary>
    /// Implements an Elasticsearch based storage provider for a bot.
    /// </summary>
    public class ElasticsearchStorage : IStorage
    {
        // Constants
        public const string IndexMappingDepthLimitSetting = "mapping.depth.limit";
        public const string RollingIndexDateFormat = "MM-dd-yyyy";

        // The list of illegal characters which should not be used in id field.
        private static readonly char[] IllegalKeyCharacters = new char[] { '\\', '?', '/', '#', ' ', '|' };

        private static readonly JsonSerializer JsonSerializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.All };

        // Prevent issues in case multiple requests arrive to create index concurrently.
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        // Name of the index.
        private readonly string indexName;

        // Determines how deep a document can be nested.
        private readonly int indexMappingDepthLimit;

        // Options for the elasticsearch storage component.
        private readonly ElasticsearchStorageOptions elasticsearchStorageOptions;

        private ElasticClient elasticClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticsearchStorage"/> class.
        /// </summary>
        /// <param name="elasticsearchStorageOptions"><see cref="ElasticsearchStorageOptions"/>.</param>
        public ElasticsearchStorage(ElasticsearchStorageOptions elasticsearchStorageOptions)
        {
            if (elasticsearchStorageOptions == null)
            {
                throw new ArgumentNullException(nameof(elasticsearchStorageOptions), "Elasticsearch storage options is required.");
            }

            if (elasticsearchStorageOptions.ElasticsearchEndpoint == null)
            {
                throw new ArgumentNullException(nameof(elasticsearchStorageOptions.ElasticsearchEndpoint), "Service endpoint for Elasticsearch is required.");
            }

            this.indexName = elasticsearchStorageOptions.IndexName ?? throw new ArgumentNullException("Index name for Elasticsearch is required.", nameof(elasticsearchStorageOptions.IndexName));
            this.indexMappingDepthLimit = elasticsearchStorageOptions.IndexMappingDepthLimit ?? 100000;

            this.elasticsearchStorageOptions = elasticsearchStorageOptions;

            InitializeSingleNodeConnectionPoolClient();
        }

        /// <summary>
        /// Deletes storage items from storage.
        /// </summary>
        /// <param name="keys">keys of the <see cref="IStoreItem"/> objects to remove from the store.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null || keys.Length == 0)
            {
                return;
            }

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            // Delete the corresponding keys.
            foreach (var key in keys)
            {
                var deleteResponse = await elasticClient.DeleteAsync<DocumentItem>(SanitizeKey(key), d => d
                .Index(indexName).Refresh(Refresh.True)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Reads storage items from storage.
        /// </summary>
        /// <param name="keys">keys of the <see cref="IStoreItem"/> objects to read from the store.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// the items read, indexed by key.</remarks>
        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null || keys.Length == 0)
            {
                // No keys passed in, no result to return.
                return new Dictionary<string, object>();
            }

            var storeItems = new Dictionary<string, object>(keys.Length);

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            foreach (var key in keys)
            {
                var searchResponse = await elasticClient.SearchAsync<DocumentItem>(s => s
                .Index(indexName)
                .Sort(ss => ss
                .Descending(p => p.Timestamp))
                .Size(1)
                .Query(q => q
                .MatchPhrase(m => m
                .Field(f => f.RealId)
                .Query(key)))).ConfigureAwait(false);

                var documentItems = searchResponse.Documents;

                foreach (var documentItem in documentItems)
                {
                    if (key == documentItem.RealId)
                    {
                        var state = documentItem.Document;
                        if (state != null)
                        {
                            storeItems.Add(key, state.ToObject<object>(JsonSerializer));
                        }
                    }
                }
            }

            return storeItems;
        }

        /// <summary>
        /// Writes storage items to storage.
        /// </summary>
        /// <param name="changes">The items to write to storage, indexed by key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (changes == null || changes.Count == 0)
            {
                return;
            }

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            foreach (var change in changes)
            {
                var newValue = change.Value;
                var newState = JObject.FromObject(newValue, JsonSerializer);
                var documentItem = new DocumentItem
                {
                    Id = SanitizeKey(change.Key),
                    RealId = change.Key,
                    Document = newState,
                    Timestamp = DateTime.Now.ToUniversalTime()
                };

                var indexResponse = await elasticClient.IndexAsync(documentItem, i => i
                .Index(indexName + "-" + DateTime.Now.ToString(RollingIndexDateFormat)).Refresh(Refresh.True)).ConfigureAwait(false);
            }
        }

        private static string SanitizeKey(string key)
        {
            var sanitizedKeyBuilder = new StringBuilder();

            // If illegal character is found remove it.
            foreach (var character in key)
            {
                if (!IllegalKeyCharacters.Contains(character))
                {
                    sanitizedKeyBuilder.Append(character);
                }
            }

            return sanitizedKeyBuilder.ToString();
        }

        /// <summary>
        /// Initializes the Elasticsearch single node connection pool client.
        /// </summary>
        private void InitializeSingleNodeConnectionPoolClient()
        {
            var connectionPool = new SingleNodeConnectionPool(elasticsearchStorageOptions.ElasticsearchEndpoint);
            CreateClient(connectionPool);
        }

        /// <summary>
        /// Creates the Elasticsearch client.
        /// </summary>
        /// <param name="connectionPool">Elasticsearch connection pool.</param>
        private void CreateClient(SingleNodeConnectionPool connectionPool)
        {
            // Instantiate connection settings from the connection pool.
            // Set JsonNetSerializer as the source serializer to use Newtonsoft.JSON serialization.
            var connectionSettings = new ConnectionSettings(connectionPool, sourceSerializer: JsonNetSerializer.Default);

            if (!string.IsNullOrEmpty(elasticsearchStorageOptions.UserName) && !string.IsNullOrEmpty(elasticsearchStorageOptions.Password))
            {
                connectionSettings = connectionSettings.BasicAuthentication(elasticsearchStorageOptions.UserName, elasticsearchStorageOptions.Password);
            }

            elasticClient = new ElasticClient(connectionSettings);
        }

        private async Task InitializeAsync()
        {
            // Check whether the index exists or not.
            var indexExistsResponse = await elasticClient.IndexExistsAsync(indexName + "-" + DateTime.Now.ToString(RollingIndexDateFormat));

            if (!indexExistsResponse.Exists)
            {
                // We don't (probably) have created the index yet. Enter the lock,
                // then check again (aka: Double-Check Lock pattern).
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (!indexExistsResponse.Exists)
                    {
                        // If the index does not exist, create a new one with the current date and alias it.
                        var createIndexResponse = await elasticClient.CreateIndexAsync(indexName + "-" + DateTime.Now.ToString(RollingIndexDateFormat), c => c
                        .Mappings(ms => ms.Map<DocumentItem>(m => m.AutoMap())).Settings(s => s.Setting(IndexMappingDepthLimitSetting, indexMappingDepthLimit))).ConfigureAwait(false);

                        await elasticClient.AliasAsync(ac => ac.Add(a => a.Index(indexName + "-" + DateTime.Now.ToString(RollingIndexDateFormat)).Alias(indexName))).ConfigureAwait(false);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }
    }
}
