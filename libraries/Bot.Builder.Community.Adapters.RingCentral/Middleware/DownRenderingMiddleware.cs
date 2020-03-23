using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.RingCentral.Helpers;
using Bot.Builder.Community.Adapters.RingCentral.Renderer;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.RingCentral.Middleware
{
    /// <summary>
    /// Middleware used to render the rich formated attachments into a format, which can be displayed in
    /// channels with fewer display capabilities, e.g. WhatsApp.
    /// This Middleware implementes a default down rendering. If there is a need for custom down rendering,
    /// this calls can be subclassed and the needed, concrte rendering methods, can be overwirtten.
    /// </summary>
    public class DownRenderingMiddleware : IMiddleware
    {
        private List<IRenderer> _renderers = new List<IRenderer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DownRenderingMiddleware"/> class.
        /// Custom renderers can be injected. A <see cref="PlainTextRenderer"/>  will be used as fallback.
        /// </summary>
        /// <param name="whatsAppRenderer">WhatsApp downrenderer instance.</param>
        public DownRenderingMiddleware(IWhatsAppRenderer whatsAppRenderer = null)
        {
            if (whatsAppRenderer != null)
            {
                _renderers.Add(whatsAppRenderer);
            }

            // add the default renderer at the end of this list
            _renderers.Add(new PlainTextRenderer());
        }

        /// <summary>
        /// Handles the messages, which are sent back to RingCentral.
        /// </summary>
        /// <param name="turnContext">Turncontext instance.</param>
        /// <param name="next">NextDelegate to invoke.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            // Hook on messages that are sent from Bot to the user
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                bool isFromRingCentral = RingCentralSdkHelper.IsActivityFromRingCentralOperator(turnContext.Activity);

                foreach (var activity in activities)
                {
                    // Send out messages from the bot to RingCentral
                    if (activity.Type == ActivityTypes.Message && !isFromRingCentral)
                    {
                        RenderActivityForThirdPartyChannel(activity);
                    }
                }
                
                // Run full pipeline
                var responses = await nextSend().ConfigureAwait(false);
                return responses;
            });

            await next(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the rendering of the message, including attachments.
        /// Attachments will be rendered as simple text and the result will be appended to thi activities text attribute.
        /// The attachments will be removed from the activity.
        /// </summary>
        /// <param name="activity">Activity instance.</param>
        private void RenderActivityForThirdPartyChannel(Activity activity)
        {
            // TODO parse text if it is not plain text
            var messageText = activity.Text;

            var attachmentsText = RenderAttachments(activity);

            // overwrite the text of the activity
            activity.Text = messageText + attachmentsText;

            //remove attachments, otherwise they will be rendered addtionally
            activity.Attachments = null;
        }

        /// <summary>
        /// Handles different types of attachments and delegates the rendering to the corresponding rendering method.
        /// </summary>
        /// <param name="activity">Activity instance.</param>
        /// <returns>String value of all the attachments text.</returns>
        private string RenderAttachments(Activity activity)
        {
            // Find a channel specifc renderer. Use the default renderer, if no custom renderer is found.
            IRenderer renderer = _renderers.Find(r => r.ChannelId.Equals(activity.ChannelId, StringComparison.InvariantCultureIgnoreCase) || r.ChannelId.Equals(PlainTextRenderer.DefaultChannelRendererId));

            var attachmentsText = new StringBuilder();

            // Handle attachments like HeroCards
            if (activity.Attachments != null)
            {
                foreach (Attachment attachment in activity.Attachments)
                {
                    switch (attachment.ContentType)
                    {
                        case HeroCard.ContentType:
                        case ThumbnailCard.ContentType:
                            var heroCard = (HeroCard)attachment.Content;
                            attachmentsText.AppendLine(renderer.RenderHeroCard(heroCard));
                            break;
                        case VideoCard.ContentType:
                        case AudioCard.ContentType:
                        case AnimationCard.ContentType:
                            var richMediaCard = (MediaCard)attachment.Content;
                            attachmentsText.AppendLine(renderer.RenderMediaCard(richMediaCard));
                            break;
                        //case "application/vnd.microsoft.card.adaptive":
                        //    break;
                        default:
                            break;
                    }
                }
            }

            return attachmentsText.ToString();
        }
    }
}
