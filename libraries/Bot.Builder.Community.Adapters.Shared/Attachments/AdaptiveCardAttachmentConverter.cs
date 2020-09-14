using System;
using System.Collections.Generic;
using AdaptiveCards;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Shared.Attachments
{
    public class AdaptiveCardAttachmentConverter : AttachmentConverterBase
    {
        private readonly IReadOnlyDictionary<string, Action<Attachment>> _converters;

        public AdaptiveCardAttachmentConverter()
        {
            _converters = new Dictionary<string, Action<Attachment>>
            {
                { AdaptiveCard.ContentType, x => Convert<AdaptiveCard>(x) },
            };
        }

        protected override string Name => nameof(AdaptiveCardAttachmentConverter);
        protected override IReadOnlyDictionary<string, Action<Attachment>> Converters => _converters;
    }
}
