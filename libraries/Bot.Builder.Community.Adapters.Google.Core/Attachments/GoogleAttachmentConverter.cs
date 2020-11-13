using System;
using System.Collections.Generic;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Microsoft.Bot.Schema;
using BasicCard = Bot.Builder.Community.Adapters.Google.Core.Model.Response.BasicCard;

namespace Bot.Builder.Community.Adapters.Google.Core.Attachments
{
    public static class DefaultGoogleAttachmentConverter
    {
        public static AttachmentConverter CreateDefault() => new AttachmentConverter(new GoogleAttachmentConverter());
    }

    public class GoogleAttachmentConverter : AttachmentConverterBase
    {
        private readonly IReadOnlyDictionary<string, Action<Attachment>> _converters ;

        public GoogleAttachmentConverter()
        {
            _converters = new Dictionary<string, Action<Attachment>>
            {
                { GoogleAttachmentContentTypes.BasicCard, x => Convert<BasicCard>(x) },
                { GoogleAttachmentContentTypes.MediaResponse, x => Convert<MediaResponse>(x) },
                { GoogleAttachmentContentTypes.TableCard, x => Convert<TableCard>(x) },
                { GoogleAttachmentContentTypes.BrowsingCarousel, x => Convert<BrowsingCarousel>(x) },
                { GoogleAttachmentContentTypes.CarouselIntent, x => Convert<CarouselIntent>(x) },
                { GoogleAttachmentContentTypes.ListIntent, x => Convert<ListIntent>(x) },
                { GoogleAttachmentContentTypes.DateTimeIntent, x => Convert<DateTimeIntent>(x) },
                { GoogleAttachmentContentTypes.PermissionsIntent, x => Convert<PermissionsIntent>(x) },
                { GoogleAttachmentContentTypes.PlaceLocationIntent, x => Convert<PlaceLocationIntent>(x) },
                { GoogleAttachmentContentTypes.ConfirmationIntent, x => Convert<ConfirmationIntent>(x) },
                { GoogleAttachmentContentTypes.NewSurfaceIntent, x => Convert<NewSurfaceIntent>(x) },
                { GoogleAttachmentContentTypes.SigninIntent, x => Convert<SigninIntent>(x) }
            };
        }

        protected override string Name => nameof(GoogleAttachmentConverter);
        protected override IReadOnlyDictionary<string, Action<Attachment>> Converters => _converters;
    }
}
