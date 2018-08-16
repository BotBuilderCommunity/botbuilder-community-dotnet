namespace Bot.Builder.Community.Dialogs.Location.Bing
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the interface the defines how the <see cref="LocationDialog"/> will query for locations.
    /// </summary>
    public interface IGeoSpatialService
    {
        /// <summary>
        /// Gets the locations asynchronously.
        /// </summary>
        /// <param name="address">The address query.</param>
        /// <returns>The found locations</returns>
        Task<LocationSet> GetLocationsByQueryAsync(string address);

        /// <summary>
        /// Gets the locations asynchronously.
        /// </summary>
        /// <param name="latitude">The point latitude.</param>
        /// <param name="longitude">The point longitude.</param>
        /// <returns>The found locations</returns>
        Task<LocationSet> GetLocationsByPointAsync(double latitude, double longitude);

        /// <summary>
        /// Gets the map image URL.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="index">The pin point index.</param>
        /// <returns></returns>
        string GetLocationMapImageUrl(Location location, int? index = null);
    }
}
