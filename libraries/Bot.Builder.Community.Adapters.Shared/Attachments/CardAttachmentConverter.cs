using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Shared.Attachments
{
    /// <summary>
    /// Convert all Microsoft.Bot.Schema Card types from blobs to their strong type.
    /// </summary>
    public class CardAttachmentConverter : AttachmentConverterBase
    {
        private readonly IReadOnlyDictionary<string, Action<Attachment>> _converters;

        public CardAttachmentConverter()
        {
            _converters = new Dictionary<string, Action<Attachment>>
            {
                { AnimationCard.ContentType, x => Convert<AnimationCard>(x) },
                { AudioCard.ContentType, x => Convert<AudioCard>(x) },
                { HeroCard.ContentType, x => Convert<HeroCard>(x) },
                { OAuthCard.ContentType, x => Convert<OAuthCard>(x) },
                { ReceiptCard.ContentType, x => Convert<ReceiptCard>(x) },
                { SigninCard.ContentType, x => Convert<SigninCard>(x) },
                { ThumbnailCard.ContentType, x => Convert<ThumbnailCard>(x) },
                { VideoCard.ContentType, x => Convert<VideoCard>(x) },
            };
        }

        protected override string Name => nameof(CardAttachmentConverter);
        protected override IReadOnlyDictionary<string, Action<Attachment>> Converters => _converters;
    }
}
