using Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Attachments
{
    public static class Extensions
    {
        public static Attachment ToAttachment(this ContentItem responseItem)
        {
            switch (responseItem)
            {
                case TableContentItem table:
                    return CreateAttachment(table, ActionsSdkAttachmentContentTypes.Table);
                case MediaContentItem media:
                    return CreateAttachment(media, ActionsSdkAttachmentContentTypes.Media);
                case CardContentItem card:
                    return CreateAttachment(card, ActionsSdkAttachmentContentTypes.Card);
                case ListContentItem list:
                    return CreateAttachment(list, ActionsSdkAttachmentContentTypes.List);
                case CollectionContentItem collection:
                    return CreateAttachment(collection, ActionsSdkAttachmentContentTypes.Collection);
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
