using Elasticsearch.Net;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.TranscriptStore.Elasticsearch
{
    public class ElasticsearchTranscriptStore : ITranscriptStore
    {
        // Constants
        public const string RollingIndexDateFormat = "MM-dd-yyyy";

        private static readonly JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        });

        // Prevent issues in case multiple requests arrive to create index concurrently.
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        // Name of the index.
        private readonly string indexName;

        // Options for the elasticsearch transcript store component.
        private readonly ElasticsearchTranscriptStoreOptions elasticsearchTranscriptStoreOptions;

        private ElasticClient elasticClient;

        public const string IndexMappingTotalFieldsLimitSetting = "mapping.total_fields.limit";

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticsearchTranscriptLogger"/> class.
        /// </summary>
        /// <param name="elasticsearchTranscriptStoreOptions"><see cref="ElasticsearchTranscriptStoreOptions"/>.</param>
        public ElasticsearchTranscriptStore(ElasticsearchTranscriptStoreOptions elasticsearchTranscriptStoreOptions)
        {
            if (elasticsearchTranscriptStoreOptions == null)
            {
                throw new ArgumentNullException(nameof(elasticsearchTranscriptStoreOptions), "Elasticsearch transcript store options is required.");
            }

            if (elasticsearchTranscriptStoreOptions.ElasticsearchEndpoint == null)
            {
                throw new ArgumentNullException(nameof(elasticsearchTranscriptStoreOptions.ElasticsearchEndpoint), "Service endpoint for Elasticsearch is required.");
            }

            this.indexName = elasticsearchTranscriptStoreOptions.IndexName ?? throw new ArgumentNullException("Index name for Elasticsearch is required.", nameof(elasticsearchTranscriptStoreOptions.IndexName));

            this.elasticsearchTranscriptStoreOptions = elasticsearchTranscriptStoreOptions;

            InitializeSingleNodeConnectionPoolClient();
        }

        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">Activity being logged.</param>
        /// <returns>A <see cref="Task"/>A task that represents the work queued to execute.</returns>
        public async Task LogActivityAsync(IActivity activity)
        {
            if (activity == null)
            {
                return;
            }

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            var documentItem = new DocumentItem
            {
                ChannelId = activity.ChannelId,
                ConversationId = activity.Conversation.Id,
                Timestamp = activity.Timestamp,
                Activity = (Activity)activity
            };

            var indexResponse = await elasticClient.IndexAsync(documentItem, i => i
                .Index(indexName + "-" + DateTime.Now.ToString(RollingIndexDateFormat)).Refresh(Refresh.True)).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a specific conversation and all of it's activities.
        /// </summary>
        /// <param name="channelId">Channel Id where conversation took place.</param>
        /// <param name="conversationId">Id of the conversation to delete.</param>
        /// <returns>A <see cref="Task"/>A task that represents the work queued to execute.</returns>
        public async Task DeleteTranscriptAsync(string channelId, string conversationId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException($"{nameof(channelId)} should not be null");
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException($"{nameof(conversationId)} should not be null");
            }

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            var deleteResponse = await elasticClient.DeleteByQueryAsync<DocumentItem>(d => d
                .Index(indexName)
                .Query(q => q
                .MatchPhrase(m => m
                .Field(f => f.ChannelId)
                .Query(channelId)))
                .Query(q => q
                .MatchPhrase(m => m
                .Field(f => f.ConversationId)
                .Query(conversationId))).Refresh(true)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get activities for a conversation (Aka the transcript).
        /// </summary>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="conversationId">Conversation Id.</param>
        /// <param name="continuationToken">Continuatuation token to page through results.</param>
        /// <param name="startDate">Earliest time to include.</param>
        /// <returns>PagedResult of activities.</returns>
        public async Task<PagedResult<IActivity>> GetTranscriptActivitiesAsync(string channelId, string conversationId, string continuationToken = null, DateTimeOffset startDate = default(DateTimeOffset))
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException($"missing {nameof(channelId)}");
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException($"missing {nameof(conversationId)}");
            }

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            int pageStart = 0;
            int pageSize = 20;

            if (continuationToken != null)
            {
                bool result = int.TryParse(continuationToken, out pageStart);

                if (!result)
                {
                    throw new InvalidOperationException(nameof(result));
                }
            }

            var pagedResult = new PagedResult<IActivity>();

            var searchResponse = await elasticClient.SearchAsync<DocumentItem>(s => s
                .Index(indexName)
                .Sort(ss => ss
                .Ascending(p => p.Timestamp))
                .From(pageStart)
                .Size(pageSize)
                .Query(q => q
                .MatchPhrase(m => m
                .Field(f => f.ChannelId)
                .Query(channelId)))
                .Query(q => q
                .MatchPhrase(m => m
                .Field(f => f.ConversationId)
                .Query(conversationId)))).ConfigureAwait(false);

            var documentItems = searchResponse.Documents;

            var pagedItems = new List<IActivity>();

            foreach (var documentItem in documentItems)
            {
                if (documentItem.Timestamp >= startDate)
                {
                    pagedItems.Add(documentItem.Activity);
                }
            }

            pagedResult.Items = pagedItems.ToArray();

            if (pagedResult.Items.Length == pageSize)
            {
                pagedResult.ContinuationToken = $"{pageStart + pageSize}";
            }
            else
            {
                pagedResult.ContinuationToken = null;
            }

            return pagedResult;
        }

        /// <summary>
        /// List conversations in the channelId.
        /// </summary>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="continuationToken">Continuation token to page through results.</param>
        /// <returns>A <see cref="Task"/> A task that represents the work queued to execute.</returns>
        public async Task<PagedResult<TranscriptInfo>> ListTranscriptsAsync(string channelId, string continuationToken = null)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException($"missing {nameof(channelId)}");
            }

            int pageStart = 0;
            int pageSize = 20;

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            if (continuationToken != null)
            {
                bool result = int.TryParse(continuationToken, out pageStart);

                if (!result)
                {
                    throw new InvalidOperationException(nameof(result));
                }
            }

            List<TranscriptInfo> conversations = new List<TranscriptInfo>();

            var pagedResult = new PagedResult<TranscriptInfo>();

            var searchResponse = await elasticClient.SearchAsync<DocumentItem>(s => s
                .Index(indexName)
                .Sort(ss => ss
                .Ascending(p => p.Timestamp))
                .From(pageStart)
                .Size(pageSize)
                .Query(q => q
                .MatchPhrase(m => m
                .Field(f => f.ChannelId)
                .Query(channelId)))).ConfigureAwait(false);

            var documentItems = searchResponse.Documents;

            foreach (var documentItem in documentItems)
            {
                var conversation = new TranscriptInfo { Id = documentItem.Id, ChannelId = documentItem.ChannelId, Created = (DateTimeOffset)documentItem.Timestamp };
                conversations.Add(conversation);
            }

            pagedResult.Items = conversations.ToArray();

            if (pagedResult.Items.Length == pageSize)
            {
                pagedResult.ContinuationToken = $"{pageStart + pageSize}";
            }
            else
            {
                pagedResult.ContinuationToken = null;
            }

            return pagedResult;
        }

        /// <summary>
        /// Initializes the Elasticsearch single node connection pool client.
        /// </summary>
        private void InitializeSingleNodeConnectionPoolClient()
        {
            var connectionPool = new SingleNodeConnectionPool(elasticsearchTranscriptStoreOptions.ElasticsearchEndpoint);
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

            if (!string.IsNullOrEmpty(elasticsearchTranscriptStoreOptions.UserName) && !string.IsNullOrEmpty(elasticsearchTranscriptStoreOptions.Password))
            {
                connectionSettings = connectionSettings.BasicAuthentication(elasticsearchTranscriptStoreOptions.UserName, elasticsearchTranscriptStoreOptions.Password);
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
                        .Mappings(ms => ms.Map<DocumentItem>(m => m.AutoMap())).Settings(s => s.Setting(IndexMappingTotalFieldsLimitSetting, 100000))).ConfigureAwait(false);

                        var aliasResponse = await elasticClient.AliasAsync(ac => ac.Add(a => a.Index(indexName + "-" + DateTime.Now.ToString(RollingIndexDateFormat)).Alias(indexName))).ConfigureAwait(false);
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
