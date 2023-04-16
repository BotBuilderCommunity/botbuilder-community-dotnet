using System;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Bot.Builder.Community.Storage.MongoDB
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures MongoDB storage support
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the storage to.</param>
        /// <param name="settings">A delegate to configure the <see cref="MongoDbStorageSettings"/>.</param>
        /// <param name="contextLifetime">The <see cref="ServiceLifetime"/> of the storage context (default is <see cref="ServiceLifetime.Singleton"/>).</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
        /// <exception cref="ArgumentException">Thrown when no types are found to scan or when the connection string or database name is not provided.</exception>
        /// <remarks>
        /// This extension method configures MongoDB storage support for the specified service collection. It requires the caller to provide at least one type to scan and a valid connection string and database name.
        /// </remarks>
        public static IServiceCollection AddMongoDbStorage(this IServiceCollection services,
            Action<MongoDbStorageSettings> settings,
            ServiceLifetime contextLifetime = ServiceLifetime.Singleton)
        {
            var config = new MongoDbStorageSettings();

            settings.Invoke(config);

            if (!config.AllowedTypes.Any())
            {
                throw new ArgumentException("No types found to scan. Supply at least one type");
            }

            if (config.Options == null
                || string.IsNullOrEmpty(config.Options.ConnectionString)
                || string.IsNullOrEmpty(config.Options.DatabaseName)
               )
            {
                throw new ArgumentException("No connection string or database name found.");
            }

            var objectSerializer = new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) || config.AllowedTypes.Contains(type));

            BsonSerializer.RegisterSerializer(objectSerializer);

            services.TryAdd(new ServiceDescriptor(typeof(IStorage), provider => new MongoDbStorage(config.Options), contextLifetime));

            return services;
        }
    }
}