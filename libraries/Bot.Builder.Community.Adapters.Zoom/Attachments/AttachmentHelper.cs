using Bot.Builder.Community.Adapters.Shared;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Zoom.Attachments
{
    public static class AttachmentHelper
    {
        public static void ConvertAttachmentContent(this Activity activity)
        {
            if (activity == null || activity.Attachments == null)
            {
                return;
            }

            foreach (var attachment in activity.Attachments)
            {
                switch (attachment.ContentType)
                {
                    case ZoomAttachmentContentTypes.MessageWithLink:
                        SharedAttachmentHelper.Convert<MessageBodyItemWithLink>(attachment);
                        break;
                    case ZoomAttachmentContentTypes.Fields:
                        SharedAttachmentHelper.Convert<FieldsBodyItem>(attachment);
                        break;
                    case ZoomAttachmentContentTypes.Dropdown:
                        SharedAttachmentHelper.Convert<DropdownBodyItem>(attachment);
                        break;
                    case ZoomAttachmentContentTypes.Attachment:
                        SharedAttachmentHelper.Convert<AttachmentBodyItem>(attachment);
                        break;
                }
            }
        }
    }
}
