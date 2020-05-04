using System;
using Bot.Builder.Community.Adapters.Zoom.Attachments;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Zoom.Helpers
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
                        Convert<MessageBodyItemWithLink>(attachment);
                        break;
                    case ZoomAttachmentContentTypes.Fields:
                        Convert<FieldsBodyItem>(attachment);
                        break;
                    case ZoomAttachmentContentTypes.Dropdown:
                        Convert<DropdownBodyItem>(attachment);
                        break;
                    case ZoomAttachmentContentTypes.Attachment:
                        Convert<AttachmentBodyItem>(attachment);
                        break;
                }
            }
        }

        private static void Convert<T>(Attachment attachment)
        {
            try
            {
                attachment.Content = attachment.ContentAs<T>();
            }
            catch (JsonException ex)
            {
                throw new ValidationException($"Failed to convert Zoom Attachment with ContentType {attachment?.ContentType} to {typeof(T).Name}", ex);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Convert the Attachment Content field to the given type. An exception is thrown if the conversion fails.
        /// </summary>
        public static T ContentAs<T>(this Attachment attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));

            if (attachment.Content == null)
            {
                return default;
            }
            if (typeof(T).IsValueType)
            {
                return (T)System.Convert.ChangeType(attachment.Content, typeof(T));
            }
            if (attachment.Content is T)
            {
                return (T)attachment.Content;
            }
            if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)System.Convert.FromBase64String(attachment.Content.ToString());
            }
            if (attachment.Content is string)
            {
                return JsonConvert.DeserializeObject<T>((string)attachment.Content);
            }
            return (T)((JObject)attachment.Content).ToObject<T>();
        }
    }
}
