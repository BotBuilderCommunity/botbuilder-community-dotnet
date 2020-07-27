using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Shared;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Alexa.Core.Attachments
{
    public static class AttachmentHelper
    {
        /// <summary>
        /// Convert all Alexa specific attachments to their correct type.
        /// </summary>
        /// <param name="activity"></param>
        public static void ConvertAttachmentContent(this Activity activity)
        {
            if (activity == null || activity.Attachments == null)
            {
                return;
            }

            foreach (var attachment in activity.Attachments)
            {
                switch (attachment.ContentType)
                {
                    case HeroCard.ContentType:
                        SharedAttachmentHelper.Convert<HeroCard>(attachment);
                        break;
                    case SigninCard.ContentType:
                        SharedAttachmentHelper.Convert<SigninCard>(attachment);
                        break;
                    case AlexaAttachmentContentTypes.Card:
                        SharedAttachmentHelper.Convert<ICard>(attachment);
                        break;
                    case AlexaAttachmentContentTypes.Directive:
                        SharedAttachmentHelper.Convert<IDirective>(attachment);
                        break;
                }
            }
        }
    }
}
