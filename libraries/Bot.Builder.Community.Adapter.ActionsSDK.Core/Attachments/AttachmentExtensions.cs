using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems;
using Bot.Builder.Community.Adapters.Shared;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Attachments
{
    public static class AttachmentExtensions
    {
        public static Attachment ToAttachment(this ContentItem responseItem)
        {
            switch (responseItem)
            {
                case TableContentItem table:
                    return SharedAttachmentHelper.CreateAttachment(table, ActionsSdkAttachmentContentTypes.Table);
                case MediaContentItem media:
                    return SharedAttachmentHelper.CreateAttachment(media, ActionsSdkAttachmentContentTypes.Media);
                case CardContentItem card:
                    return SharedAttachmentHelper.CreateAttachment(card, ActionsSdkAttachmentContentTypes.Card);
                case ListContentItem list:
                    return SharedAttachmentHelper.CreateAttachment(list, ActionsSdkAttachmentContentTypes.List);
                case CollectionContentItem collection:
                    return SharedAttachmentHelper.CreateAttachment(collection, ActionsSdkAttachmentContentTypes.Collection);
                default:
                    return null;
            }
        }
    }
}
