using System;
using System.Collections.Generic;
using System.Text;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BasicCard = Bot.Builder.Community.Adapters.Google.Core.Model.Response.BasicCard;

namespace Bot.Builder.Community.Adapters.Google.Core.Attachments
{
    public static class Extensions
    {
        public static Attachment ToAttachment(this ResponseItem responseItem)
        {
            switch (responseItem)
            {
                case TableCard tableCard:
                    return CreateAttachment(tableCard, GoogleAttachmentContentTypes.TableCard);
                case MediaResponse mediaResponse:
                    return CreateAttachment(mediaResponse, GoogleAttachmentContentTypes.MediaResponse);
                case BasicCard basicCard:
                    return CreateAttachment(basicCard, GoogleAttachmentContentTypes.BasicCard);
                case BrowsingCarousel browsingCarousel:
                    return CreateAttachment(browsingCarousel, GoogleAttachmentContentTypes.BrowsingCarousel);
                default:
                    return null;
            }
        }

        public static Attachment ToAttachment(this SystemIntent intent)
        {
            switch (intent)
            {
                case CarouselIntent carouselIntent:
                    return CreateAttachment(carouselIntent, GoogleAttachmentContentTypes.CarouselIntent);
                case ListIntent listIntent:
                    return CreateAttachment(listIntent, GoogleAttachmentContentTypes.ListIntent);
                case ConfirmationIntent confirmationIntent:
                    return CreateAttachment(confirmationIntent, GoogleAttachmentContentTypes.ConfirmationIntent);
                case DateTimeIntent dateTimeIntent:
                    return CreateAttachment(dateTimeIntent, GoogleAttachmentContentTypes.DateTimeIntent);
                case PermissionsIntent permissionsIntent:
                    return CreateAttachment(permissionsIntent, GoogleAttachmentContentTypes.PermissionsIntent);
                case PlaceLocationIntent placeLocationIntent:
                    return CreateAttachment(placeLocationIntent, GoogleAttachmentContentTypes.PlaceLocationIntent);
                case NewSurfaceIntent newSurfaceIntent:
                    return CreateAttachment(newSurfaceIntent, GoogleAttachmentContentTypes.NewSurfaceIntent);
                default:
                    return null;
            }
        }

        private static Attachment CreateAttachment<T>(T card, string contentType)
        {
            return new Attachment
            {
                Content = JObject.FromObject(card, new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore }),
                ContentType = contentType,
            };
        }
    }
}
