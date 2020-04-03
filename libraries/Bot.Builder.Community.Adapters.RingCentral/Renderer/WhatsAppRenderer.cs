using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.RingCentral.Renderer
{
    /// <summary>
    /// Default WhatsApp renderer, can be overwritten by a custom WhatsAppRenderer.
    /// </summary>
    public class WhatsAppRenderer : PlainTextRenderer, IWhatsAppRenderer
    {
        public WhatsAppRenderer()
        {
        }

        public override string ChannelId => RingCentralChannels.WhatsApp;

        public override string RenderHeroCard(HeroCard heroCard)
        {
            // Add images
            var imageUrls = heroCard.Images?.Select(x => x.Url).ToList();

            // Generate body
            StringBuilder body = new StringBuilder();
            body.AppendLine("*" + heroCard.Title + "*");
            body.AppendLine("_" + heroCard.Subtitle + "_");
            body.AppendLine(heroCard.Text);

            // Add buttons
            if (heroCard.Buttons != null && heroCard.Buttons.Count > 0)
            {
                body = body.Append(ButtonsToText(heroCard.Buttons));
            }

            var result = body.ToString();

            return result;
        }

        public override string RenderMediaCard(MediaCard mediaCard)
        {
            // Add images
            var mediaUrls = new List<string> { mediaCard.Media?.First().Url };

            // Generate body
            StringBuilder body = new StringBuilder();
            body.AppendLine("*" + mediaCard.Title + "*");
            body.AppendLine("_" + mediaCard.Subtitle + "_");
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
        /// <param name="buttons">Collection of buttons.</param>
        /// <returns>String response.</returns>
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
