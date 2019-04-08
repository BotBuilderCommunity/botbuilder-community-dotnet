using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Storage.EntityFramework
{
    /// <summary>
    /// Implements an EntityFramework based storage provider for a bot.
    /// </summary>
    public class EntityFrameworkStorage : IStorage
    {
        private static readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        
        private bool _checkedConnection;
        private readonly EntityFrameworkStorageOptions _storageOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkStorage"/> class.
        /// using the provided connection string.
        /// </summary>
        /// <param name="connectionString">Entity Framework database connection string.</param>
        public EntityFrameworkStorage(string connectionString)
            : this(new EntityFrameworkStorageOptions() { ConnectionString = connectionString })
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkStorage"/> class.
        /// </summary>
        /// <param name="options">Entity Framework options <see cref="EntityFrameworkStorageOptions"/> class.</param>
        public EntityFrameworkStorage(EntityFrameworkStorageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                throw new ArgumentNullException(nameof(options.ConnectionString));
            }

            _storageOptions = options;
        }

        /// <summary>
        /// Get a BotDataContext will by default use the connection string provided during EntityFrameworkStorage construction.
        /// </summary>
        public virtual BotDataContext GetBotDataContext => new BotDataContext(_storageOptions.ConnectionString);

        /// <summary>
        /// Deletes storage items from storage.
        /// </summary>
        /// <param name="keys">keys of the <see cref="IStoreItem"/> objects to remove from the store.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ReadAsync(string[], CancellationToken)"/>
        /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Length == 0)
            {
                return;
            }

            // Ensure Initialization has been run
            await EnsureConnection().ConfigureAwait(false);

            using (var context = GetBotDataContext)
            {
                context.RemoveRange(context.BotDataEntity.Where(item => keys.Contains(item.RealId)));
                await context.SaveChangesAsync();
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
        /// <seealso cref="DeleteAsync(string[], CancellationToken)"/>
        /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Length == 0)
            {
                // No keys passed in, no result to return.
                return new Dictionary<string, object>();
            }

            // Ensure we have checked for possible connection issues
            await EnsureConnection().ConfigureAwait(false);

            var storeItems = new Dictionary<string, object>(keys.Length);

            using (var database = GetBotDataContext)
            {
                var query = (from item in database.BotDataEntity
                             where keys.Any(k => k == item.RealId)
                             select new { item.RealId, item.Document });
                
                foreach (var item in query)
                {
                    var jObject = JObject.Parse(item.Document).ToObject(typeof(object), _jsonSerializer);
                    storeItems.Add(item.RealId, jObject);
                }
                
                return storeItems;
            }
        }

        /// <summary>
        /// Writes storage items to storage.
        /// </summary>
        /// <param name="changes">The items to write to storage, indexed by key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="DeleteAsync(string[], CancellationToken)"/>
        /// <seealso cref="ReadAsync(string[], CancellationToken)"/>
        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            if (changes.Count == 0)
            {
                return;
            }

            // Ensure Initialization has been run
            await EnsureConnection().ConfigureAwait(false);
            
            using (var context = GetBotDataContext)
            {
                // Begin a transaction using the isolation level provided in Storage Options
                var transaction = context.Database.BeginTransaction(_storageOptions.TransactionIsolationLevel);
                
                var existingItems = context.BotDataEntity.Where(item => changes.Keys.Contains(item.RealId)).ToDictionary(d => (d as BotDataEntity).RealId);

                foreach (var change in changes)
                {
                    var json = JObject.FromObject(change.Value, _jsonSerializer);
                    var existingItem = existingItems.FirstOrDefault(i => i.Key == change.Key).Value;

                    if (existingItem != null)
                    {
                        existingItem.Document = json.ToString(Formatting.None);
                        existingItem.Timestamp = DateTimeOffset.UtcNow;
                    }
                    else
                    {
                        var newItem = new BotDataEntity
                        {
                            RealId = change.Key,
                            Document = json.ToString(Formatting.None),
                            Timestamp = DateTimeOffset.UtcNow,
                        };
                        await context.BotDataEntity.AddAsync(newItem);
                    }
                }

                context.SaveChanges();
                transaction.Commit();
            }
        }

        /// <summary>
        /// Ensures a database connection is possible.
        /// </summary>
        /// <remarks>
        /// Will throw ArgumentException if the database cannot be created to.
        /// </remarks>
        private async Task EnsureConnection()
        {
            // In the steady-state case, we'll already have verified the database is setup.
            if (!_checkedConnection)
            {
                using (var context = GetBotDataContext)
                {
                    if (!await context.Database.CanConnectAsync())
                        throw new ArgumentException("The sql database defined in the connection has not been created. See https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/libraries/Bot.Builder.Community.Storage.EntityFramework");
                }
                _checkedConnection = true;
            }
        }
    }
}
