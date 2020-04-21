using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Allows fine grained configuration of RingCentral services.
    /// </summary>
    internal class RingCentralBuilder : IRingCentralBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RingCentralBuilder"/> class.
        /// Initializes a new <see cref="RingCentralBuilder"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        public RingCentralBuilder(IServiceCollection services)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            Services = services;
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }
    }
}
