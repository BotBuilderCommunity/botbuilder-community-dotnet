using System;
using System.Collections.Generic;
using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Alexa.Core.Attachments
{
    public static class DefaultAlexaAttachmentConverter
    {
        public static AttachmentConverter CreateDefault() => new AttachmentConverter(new AlexaAttachmentConverter());
    }

    public class AlexaAttachmentConverter : AttachmentConverterBase
    {
        private readonly IReadOnlyDictionary<string, Action<Attachment>> _converters;

        public AlexaAttachmentConverter()
        {
            _converters = new Dictionary<string, Action<Attachment>>
            {
                { AlexaAttachmentContentTypes.Card, Convert<ICard> },
                { AlexaAttachmentContentTypes.Directive, Convert<IDirective> }
            };
        }

        protected override string Name => nameof(AlexaAttachmentConverter);
        protected override IReadOnlyDictionary<string, Action<Attachment>> Converters => _converters;
    }
}
