using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.RingCentral.Renderer
{
    /// <summary>
    /// Default implementation of the <see cref="IRenderer"/>.
    /// This class can also be subclassed by channel specific renderers, if they
    /// only do a custom implementation for a specify Type, e.g. only for HeroCard.
    /// </summary>
    public class PlainTextRenderer : IRenderer
    {
        public const string DefaultChannelRendererId = "defaultChannelRenderer";

        public PlainTextRenderer()
        {
        }

        public virtual string ChannelId => DefaultChannelRendererId;

        public virtual string RenderHeroCard(HeroCard heroCard)
        {
            // Add images
            var imageUrls = heroCard.Images?.Select(x => x.Url).ToList();

            // Generate body
            StringBuilder body = new StringBuilder();
            body.AppendLine(heroCard.Title);
            body.AppendLine(heroCard.Subtitle);
            body.AppendLine(heroCard.Text);

            // Add buttons
            if (heroCard.Buttons != null && heroCard.Buttons.Count > 0)
            {
                body = body.Append(ButtonsToText(heroCard.Buttons));
            }

            var result = body.ToString();

            return result;
        }

        public virtual string RenderMediaCard(MediaCard mediaCard)
        {
            // Add images
            var mediaUrls = new List<string> { mediaCard.Media?.First().Url };

            // Generate body
            StringBuilder body = new StringBuilder();
            body.AppendLine(mediaCard.Title);
            body.AppendLine(mediaCard.Subtitle);
            body.AppendLine(mediaCard.Text);

            // Add buttons
            if (mediaCard.Buttons != null && mediaCard.Buttons.Count > 0)
            {
                body = body.Append(ButtonsToText(mediaCard.Buttons));
            }

            var result = body.ToString();

            return result;
        }

        /// <summary>
        /// Renders Buttons as simple Text, using MarkDown Formatting.
        /// </summary>
        /// <param name="buttons">The collection of buttons.</param>
        /// <returns>String of button text.</returns>
        private string ButtonsToText(IList<CardAction> buttons)
        {
            var builder = new StringBuilder();

            if (buttons != null)
            {
                foreach (var button in buttons)
                {
                    builder.AppendLine($"- {button.Title}");
                }
            }

            return builder.ToString();
        }
    }
}
