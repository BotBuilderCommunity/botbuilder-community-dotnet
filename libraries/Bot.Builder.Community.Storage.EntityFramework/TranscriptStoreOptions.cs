namespace Bot.Builder.Community.Storage.EntityFramework
{
    /// <summary>
    /// Entity Framework Transcript Options.
    /// </summary>
    public class TranscriptStoreOptions
    {
        /// <summary>
        /// Gets or sets the connection string to use while creating TranscriptContext.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the total records to return from the store per page.
        /// </summary>
        /// <remarks>
        /// Default 20
        /// </remarks>
        public int PageSize => 20;
    }
}
