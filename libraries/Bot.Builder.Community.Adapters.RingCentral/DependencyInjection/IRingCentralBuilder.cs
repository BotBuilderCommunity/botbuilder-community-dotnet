namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An interface for configuring RingCentral services.
    /// </summary>
    public interface IRingCentralBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> where RingCentral services are configured.
        /// </summary>
        /// <value>
        /// The Services Collection.
        /// </value>
        IServiceCollection Services { get; }
    }
}
