using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Microsoft.Bot.Schema;
using BasicCard = Bot.Builder.Community.Adapters.Google.Core.Model.Response.BasicCard;

namespace Bot.Builder.Community.Adapters.Google.Core.Attachments
{
    public static class AttachmentExtensions
    {
        public static Attachment ToAttachment(this ResponseItem responseItem)
        {
            switch (responseItem)
            {
                case TableCard tableCard:
                    return SharedAttachmentHelper.CreateAttachment(tableCard, GoogleAttachmentContentTypes.TableCard);
                case MediaResponse mediaResponse:
                    return SharedAttachmentHelper.CreateAttachment(mediaResponse, GoogleAttachmentContentTypes.MediaResponse);
                case BasicCard basicCard:
                    return SharedAttachmentHelper.CreateAttachment(basicCard, GoogleAttachmentContentTypes.BasicCard);
                case BrowsingCarousel browsingCarousel:
                    return SharedAttachmentHelper.CreateAttachment(browsingCarousel, GoogleAttachmentContentTypes.BrowsingCarousel);
                default:
                    return null;
            }
        }

        public static Attachment ToAttachment(this SystemIntent intent)
        {
            switch (intent)
            {
                case CarouselIntent carouselIntent:
                    return SharedAttachmentHelper.CreateAttachment(carouselIntent, GoogleAttachmentContentTypes.CarouselIntent);
                case ListIntent listIntent:
                    return SharedAttachmentHelper.CreateAttachment(listIntent, GoogleAttachmentContentTypes.ListIntent);
                case ConfirmationIntent confirmationIntent:
                    return SharedAttachmentHelper.CreateAttachment(confirmationIntent, GoogleAttachmentContentTypes.ConfirmationIntent);
                case DateTimeIntent dateTimeIntent:
                    return SharedAttachmentHelper.CreateAttachment(dateTimeIntent, GoogleAttachmentContentTypes.DateTimeIntent);
                case PermissionsIntent permissionsIntent:
                    return SharedAttachmentHelper.CreateAttachment(permissionsIntent, GoogleAttachmentContentTypes.PermissionsIntent);
                case PlaceLocationIntent placeLocationIntent:
                    return SharedAttachmentHelper.CreateAttachment(placeLocationIntent, GoogleAttachmentContentTypes.PlaceLocationIntent);
                case NewSurfaceIntent newSurfaceIntent:
                    return SharedAttachmentHelper.CreateAttachment(newSurfaceIntent, GoogleAttachmentContentTypes.NewSurfaceIntent);
                default:
                    return null;
            }
        }
    }
}
