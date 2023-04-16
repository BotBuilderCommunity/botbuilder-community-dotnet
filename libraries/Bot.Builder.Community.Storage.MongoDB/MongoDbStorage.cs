using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Storage.MongoDB
{
    public class MongoDbStorage: IStorage
    {
        private readonly MongoDbStorageOptions _options;
        private readonly MongoClient _mongoClient;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbStorage"/> class.
        /// </summary>
        /// <param name="options">MongoDb options <see cref="MongoDbStorage"/> class.</param>
        public MongoDbStorage(MongoDbStorageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                throw new ArgumentNullException(nameof(options.ConnectionString));
            }

            _options = options;

            _mongoClient = new MongoClient(options.ConnectionString);

        }

        /// <summary>
        /// Reads storage items from storage.
        /// </summary>
        /// <param name="keys">keys of the <see cref="IStoreItem"/> objects to read.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// the items read, indexed by key.</remarks>
        /// <seealso cref="DeleteAsync(string[], CancellationToken)"/>
        /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = new CancellationToken())
        {
            var filter = Builders<StorageEntry>.Filter
                .In(o => o.Id, keys);
            
            var result = await GetCollection().Find(filter).ToListAsync(cancellationToken: cancellationToken);
            
            return result.ToDictionary(x => x.Id, x => x.Data);
        }

        /// <summary>
        /// Writes storage items to storage.
        /// </summary>
        /// <param name="changes">The items to write, indexed by key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="DeleteAsync(string[], CancellationToken)"/>
        /// <seealso cref="ReadAsync(string[], CancellationToken)"/>
        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = new CancellationToken())
        {
            var collection = GetCollection();
            foreach (var change in changes)
            {
                var filter = Builders<StorageEntry>.Filter.Eq(o => o.Id, change.Key);
                var update = Builders<StorageEntry>.Update.Set(o => o.Data, change.Value);
                var options = new UpdateOptions { IsUpsert = true };
                await collection.UpdateOneAsync(filter, update, options, cancellationToken);
            }
        }

        /// <summary>
        /// Writes storage items to storage.
        /// </summary>
        /// <param name="changes">The items to write, indexed by key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="DeleteAsync(string[], CancellationToken)"/>
        /// <seealso cref="ReadAsync(string[], CancellationToken)"/>
        public Task DeleteAsync(string[] keys, CancellationToken cancellationToken = new CancellationToken())
        {
            
            var filter = Builders<StorageEntry>.Filter
                .In(restaurant => restaurant.Id, keys);
            
            return GetCollection().DeleteManyAsync(filter, cancellationToken);
        }
        
        private IMongoCollection<StorageEntry> GetCollection()
        {
            var database = _mongoClient.GetDatabase(_options.DatabaseName);
            var collection = database.GetCollection<StorageEntry>(_options.CollectionName);
            return collection;
        }
        private class StorageEntry
        {
            public string Id { get; set; }
            public object Data { get; set; }
        }
    }
}