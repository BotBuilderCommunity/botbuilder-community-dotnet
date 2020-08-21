using System;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Bot.Builder.Community.Adapters.Shared;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BasicCard = Bot.Builder.Community.Adapters.Google.Core.Model.Response.BasicCard;

namespace Bot.Builder.Community.Adapters.Google.Core.Attachments
{
    public static class AttachmentHelper
    {
        /// <summary>
        /// Convert all Google specific attachments to their correct type.
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
                    case GoogleAttachmentContentTypes.BasicCard:
                        SharedAttachmentHelper.Convert<BasicCard>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.MediaResponse:
                        SharedAttachmentHelper.Convert<MediaResponse>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.TableCard:
                        SharedAttachmentHelper.Convert<TableCard>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.BrowsingCarousel:
                        SharedAttachmentHelper.Convert<BrowsingCarousel>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.CarouselIntent:
                        SharedAttachmentHelper.Convert<CarouselIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.ListIntent:
                        SharedAttachmentHelper.Convert<ListIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.DateTimeIntent:
                        SharedAttachmentHelper.Convert<DateTimeIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.PermissionsIntent:
                        SharedAttachmentHelper.Convert<PermissionsIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.PlaceLocationIntent:
                        SharedAttachmentHelper.Convert<PlaceLocationIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.ConfirmationIntent:
                        SharedAttachmentHelper.Convert<ConfirmationIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.NewSurfaceIntent:
                        SharedAttachmentHelper.Convert<NewSurfaceIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.SigninIntent:
                        SharedAttachmentHelper.Convert<SigninIntent>(attachment);
                        break;
                }
            }
        }
    }
}
