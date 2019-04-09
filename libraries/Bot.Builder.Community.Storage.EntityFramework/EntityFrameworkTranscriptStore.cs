using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Storage.EntityFramework
{
    /// <summary>
    /// The Entity Framework transcript store stores transcripts in Sql Server.
    /// </summary>
    /// <remarks>
    /// Each activity is stored as json in the Activity field.
    /// </remarks>
    public class EntityFrameworkTranscriptStore : ITranscriptStore
    {
        private static readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        });

        private TranscriptStoreOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkTranscriptStore"/> class.
        /// </summary>
        /// <param name="connectionstring">Connection string to connect to Sql Server Storage.</param>
        public EntityFrameworkTranscriptStore(string connectionString)
            :this(new TranscriptStoreOptions() { ConnectionString = connectionString})
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkTranscriptStore"/> class.
        /// </summary>
        /// <param name="options">Options to use for the Transcript Store <see cref="TranscriptStoreOptions"/></param>
        public EntityFrameworkTranscriptStore(TranscriptStoreOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                throw new ArgumentNullException(nameof(options.ConnectionString) + " cannot be empty.");
            }
            _options = options;
        }

        /// <summary>
        /// Get a TranscriptContext will by default use the connection string provided during EntityFrameworkTranscriptStore construction.
        /// </summary>
        public virtual TranscriptContext GetTranscriptContext => new TranscriptContext(_options.ConnectionString);

        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">Activity being logged.</param>
        /// <returns>A <see cref="Task"/>A task that represents the work queued to execute.</returns>
        public async Task LogActivityAsync(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);

            using (var context = GetTranscriptContext)
            {
                var transcript = new TranscriptEntity()
                {
                    Channel = activity.ChannelId,
                    Conversation = activity.Conversation.Id,
                    Activity = JsonConvert.SerializeObject(activity)
                };
                await context.Transcript.AddAsync(transcript);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get activities for a conversation (Aka the transcript).
        /// </summary>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="conversationId">Conversation Id.</param>
        /// <param name="continuationToken">Continuatuation token to page through results. (Id of last record returned)</param>
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

            int continuationId = 0;
            if (!string.IsNullOrEmpty(continuationToken))
            {
                if (!int.TryParse(continuationToken, out continuationId))
                {
                    throw new ArgumentException(nameof(continuationToken) + " must be an integer");
                }
            }

            var pagedResult = new PagedResult<IActivity>();

            using (var context = GetTranscriptContext)
            {
                var query = context.Transcript.Where(t => t.Channel == channelId && t.Conversation == conversationId);
                // Filter on startDate, if present
                if (startDate != default(DateTimeOffset))
                {
                    query = query.Where(t => t.Timestamp >= startDate);
                }

                // Filter on continuationToken if present
                if (!string.IsNullOrEmpty(continuationToken))
                {
                    query = query.Where(t => t.Id > continuationId);
                }

                var finalItems = query.OrderBy(i => i.Id).Take(_options.PageSize).Select(i => new { i.Id, i.Activity }).ToArray();
                // Take only PageSize, and convert to Activities
                pagedResult.Items = finalItems.Select(i => JsonConvert.DeserializeObject<Activity>(i.Activity)).ToArray();

                if (pagedResult.Items.Length == _options.PageSize)
                {
                    pagedResult.ContinuationToken = finalItems.Last().Id.ToString();
                }
            }

            return pagedResult;
        }

        /// <summary>
        /// List conversations in the channelId.
        /// </summary>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="continuationToken">Continuatuation token to page through results.</param>
        /// <returns>A <see cref="Task"/> A task that represents the work queued to execute.</returns>
        public async Task<PagedResult<TranscriptInfo>> ListTranscriptsAsync(string channelId, string continuationToken = null)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException($"missing {nameof(channelId)}");
            }

            DateTimeOffset continuationDate = default(DateTimeOffset);
            if (!string.IsNullOrEmpty(continuationToken))
            {
                if (!DateTimeOffset.TryParse(continuationToken, out continuationDate))
                {
                    throw new ArgumentException(nameof(continuationToken) + " must be an DateTimeOffset");
                }
            }
            
            var pagedResult = new PagedResult<TranscriptInfo>();

            using (var context = GetTranscriptContext)
            {
                var query = context.Transcript.Where(t => t.Channel == channelId);

                // Get all conversation.ids with thier min Timestamps
                var items = (from p in query
                             group p by p.Conversation into grp
                             let timestamp = grp.Min(p => p.Timestamp)
                             let conversationId = grp.Key

                             from p in grp
                             where p.Conversation == conversationId && p.Timestamp == timestamp
                             select new { p.Conversation, p.Timestamp });

                // Filter on continuationToken if present
                if (!string.IsNullOrEmpty(continuationToken))
                {
                    // TODO: what if two activities have the same timestamp??? is that possible???
                    items = items.Where(i => i.Timestamp > continuationDate);
                }

                // Take only PageSize, and convert to Transcript Info
                var finalItems = items.OrderBy(i => i.Timestamp).Take(_options.PageSize);
                pagedResult.Items = finalItems.Select(i => new TranscriptInfo() { ChannelId = channelId, Id = i.Conversation, Created = i.Timestamp }).ToArray();

                // Set ContinuationToken to last date 
                if (pagedResult.Items.Length == _options.PageSize)
                {
                    pagedResult.ContinuationToken = finalItems.OrderByDescending(i => i.Timestamp).First().Timestamp.ToString();
                }
            }

            return pagedResult;
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

            using (var context = GetTranscriptContext)
            {
                context.RemoveRange(context.Transcript.Where(item => item.Conversation == conversationId && item.Channel == channelId));
                await context.SaveChangesAsync();
            }
        }
    }
}
