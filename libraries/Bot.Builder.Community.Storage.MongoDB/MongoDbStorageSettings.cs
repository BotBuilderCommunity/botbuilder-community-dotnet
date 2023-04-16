using System;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Storage.MongoDB
{
    public class MongoDbStorageSettings
    {
        public Type[] AllowedTypes { get; private set; }

        public MongoDbStorageOptions Options { get; private set; }

        /// <summary>
        /// Registers the specified types for MongoDB storage.
        /// </summary>
        /// <param name="types">An array of <see cref="Type"/> objects to be registered for storage.</param>
        /// <returns>The current <see cref="MongoDbStorageSettings"/> instance, allowing method calls to be chained.</returns>
        /// <remarks>
        /// This method sets the <see cref="AllowedTypes"/> property with the provided types. It is useful for configuring the MongoDB storage settings with the desired types that need to be stored in the database.
        /// </remarks>

        public MongoDbStorageSettings RegisterTypes(params Type[] types)
        {
            AllowedTypes = types;
            return this;
        }

        /// <summary>
        /// Configures the <see cref="MongoDbStorageOptions"/> using the provided <see cref="IConfigurationSection"/>.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfigurationSection"/> to bind the options to.</param>
        /// <returns>The current <see cref="MongoDbStorageSettings"/> instance, allowing method calls to be chained.</returns>
        /// <remarks>
        /// This method creates a new instance of <see cref="MongoDbStorageOptions"/> and binds the provided configuration section to it. The resulting options are then used to configure the MongoDB storage settings.
        /// </remarks>
        /// <example>
        /// ...
        /// "ConnectionString": "...",
        /// "DatabaseName": "...",
        /// "CollectionName": "...",
        /// ...
        /// </example>
        public MongoDbStorageSettings ConfigureOptions(IConfigurationSection configuration)
        {
            Options = new MongoDbStorageOptions();
            configuration.Bind(Options);
            return this;
        }

        /// <summary>
        /// Configures the <see cref="MongoDbStorageOptions"/> using the provided <see cref="MongoDbStorageOptions"/> instance.
        /// </summary>
        /// <param name="options">The <see cref="MongoDbStorageOptions"/> instance to use for configuration.</param>
        /// <returns>The current <see cref="MongoDbStorageSettings"/> instance, allowing method calls to be chained.</returns>
        /// <remarks>
        /// This method sets the <see cref="Options"/> property with the provided <see cref="MongoDbStorageOptions"/> instance. It is useful for configuring the MongoDB storage settings using a preconfigured options object.
        /// </remarks>
        public MongoDbStorageSettings ConfigureOptions(MongoDbStorageOptions options)
        {
            Options = options;
            return this;
        }
        
        /// <summary>
        /// Configures the <see cref="MongoDbStorageOptions"/> using the provided connection string, database name, and optionally, collection name.
        /// </summary>
        /// <param name="connectionString">The connection string for the MongoDB instance.</param>
        /// <param name="databaseName">The name of the database to be used for storage.</param>
        /// <param name="collectionName">The optional name of the collection to be used for storage (default is null).</param>
        /// <returns>The current <see cref="MongoDbStorageSettings"/> instance, allowing method calls to be chained.</returns>
        /// <remarks>
        /// This method creates a new instance of <see cref="MongoDbStorageOptions"/> and sets the provided connection string, database name, and collection name (if provided). The resulting options are then used to configure the MongoDB storage settings.
        /// </remarks>
        public MongoDbStorageSettings ConfigureOptions(
            string connectionString, 
            string databaseName,
            string collectionName = null)
        {
            Options = new MongoDbStorageOptions
            {
                ConnectionString = connectionString,
                DatabaseName = databaseName,
                CollectionName = collectionName
            };
            return this;
        }
    }
}