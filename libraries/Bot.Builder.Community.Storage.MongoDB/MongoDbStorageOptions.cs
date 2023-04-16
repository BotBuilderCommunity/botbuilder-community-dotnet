namespace Bot.Builder.Community.Storage.MongoDB
{
    /// <summary>
    /// Represents the MongoDB storage configuration options.
    /// </summary>
    /// <remarks>
    /// This class is used to configure the settings for connecting to a MongoDB instance, specifying the database and collection to be used for storage.
    /// </remarks>
    public class MongoDbStorageOptions
    {
        /// <summary>
        /// Gets or sets the connection string for the MongoDB instance.
        /// </summary>
        /// <value>The connection string.</value>
        /// <remarks>
        /// The connection string is used to specify the location and authentication details for the MongoDB instance.
        /// </remarks>
        public string ConnectionString { get; set; }
    
        /// <summary>
        /// Gets or sets the name of the database to be used for storage.
        /// </summary>
        /// <value>The name of the database.</value>
        /// <remarks>
        /// The database name is used to determine which database within the MongoDB instance should be used for storing data.
        /// </remarks>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection to be used for storage.
        /// </summary>
        /// <value>The name of the collection. Defaults to "StateData".</value>
        /// <remarks>
        /// The collection name is used to determine which collection within the specified database should be used for storing data. If not specified, the default value is "StateData".
        /// </remarks>
        public string CollectionName { get; set; } = "StateData";
    }

}