using System;
using System.Collections.Generic;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Zoom.Attachments
{
    public static class DefaultZoomAttachmentConverter
    {
        public static AttachmentConverter CreateDefault() => new AttachmentConverter(new ZoomAttachmentConverter());
    }

    public class ZoomAttachmentConverter : AttachmentConverterBase
    {
        private readonly IReadOnlyDictionary<string, Action<Attachment>> _converters;

        public ZoomAttachmentConverter()
        {
            _converters = new Dictionary<string, Action<Attachment>>
            {
                { ZoomAttachmentContentTypes.MessageWithLink, x => Convert<MessageBodyItemWithLink>(x) },
                { ZoomAttachmentContentTypes.Fields, x => Convert<FieldsBodyItem>(x) },
                { ZoomAttachmentContentTypes.Dropdown, x => Convert<DropdownBodyItem>(x) },
                { ZoomAttachmentContentTypes.Attachment, x => Convert<AttachmentBodyItem>(x) }
            };
        }

        protected override string Name => nameof(ZoomAttachmentConverter);
        protected override IReadOnlyDictionary<string, Action<Attachment>> Converters => _converters;
    }
}
