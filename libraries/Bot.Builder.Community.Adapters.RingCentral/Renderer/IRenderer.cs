using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.RingCentral.Renderer
{
    /// <summary>
    /// Definition of a renderer, for a cutom channel.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// The ID of the channel, for which a concrete renderer is responsible eg. WhatsApp
        /// </summary>
        string ChannelId { get; }

        /// <summary>
        /// Renders a <see cref="HeroCard"/> to a simple string representation.
        /// </summary>
        /// <param name="heroCard">Herocard instance.</param>
        /// <returns>HeroCard as a simple string.</returns>
        string RenderHeroCard(HeroCard heroCard);

        /// <summary>
        /// Renders a <see cref="MediaCard"/> to a simple string representation.
        /// </summary>
        /// <param name="mediaCard">Mediacard instance.</param>
        /// <returns>MediaCard as a simple string.</returns>
        string RenderMediaCard(MediaCard mediaCard);
    }
}
