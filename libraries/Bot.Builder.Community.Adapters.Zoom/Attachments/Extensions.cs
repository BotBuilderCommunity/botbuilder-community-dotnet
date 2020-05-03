using Bot.Builder.Community.Adapters.Zoom.Attachments;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Zoom.Attachments
{
    public static class Extensions
    {
        public static Attachment ToAttachment(this BodyItem bodyItem)
        {
            switch (bodyItem)
            {
                case MessageItemWithLink messageItemWithLink:
                    return CreateAttachment(messageItemWithLink, ZoomAttachmentContentTypes.MessageWithLink);
                case DropdownItem dropdownItem:
                    return CreateAttachment(dropdownItem, ZoomAttachmentContentTypes.Dropdown);
                case AttachmentItem attachmentItem:
                    return CreateAttachment(attachmentItem, ZoomAttachmentContentTypes.Attachment);
                case FieldsItem fieldsItem:
                    return CreateAttachment(fieldsItem, ZoomAttachmentContentTypes.Fields);
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
