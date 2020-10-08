using System;
using System.Collections.Generic;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Attachments
{
    public static class DefaultActionsSdkAttachmentConverter
    {
        public static AttachmentConverter CreateDefault() => new AttachmentConverter(new ActionsSdkAttachmentConverter());
    }

    public class ActionsSdkAttachmentConverter : AttachmentConverterBase
    {
        private readonly IReadOnlyDictionary<string, Action<Attachment>> _converters;

        public ActionsSdkAttachmentConverter()
        {
            _converters = new Dictionary<string, Action<Attachment>>
            {
                { ActionsSdkAttachmentContentTypes.Card, Convert<CardContentItem> },
                { ActionsSdkAttachmentContentTypes.Table, Convert<TableContentItem> },
                { ActionsSdkAttachmentContentTypes.Media, Convert<MediaContentItem> },
                { ActionsSdkAttachmentContentTypes.Collection, Convert<CollectionContentItem> },
                { ActionsSdkAttachmentContentTypes.List, Convert<ListContentItem> }
            };
        }

        protected override string Name => nameof(ActionsSdkAttachmentConverter);
        protected override IReadOnlyDictionary<string, Action<Attachment>> Converters => _converters;
    }
}
