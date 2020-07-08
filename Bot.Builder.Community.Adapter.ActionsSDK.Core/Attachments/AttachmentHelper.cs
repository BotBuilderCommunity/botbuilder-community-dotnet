using System;
using Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Attachments
{
    public static class AttachmentHelper
    {
        /// <summary>
        /// Convert all Actions SDK specific attachments to their correct type.
        /// </summary>
        /// <param name="activity"></param>
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
                    case HeroCard.ContentType:
                        Convert<HeroCard>(attachment);
                        break;
                    case SigninCard.ContentType:
                        Convert<SigninCard>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.Card:
                        Convert<CardContentItem>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.Table:
                        Convert<TableContentItem>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.Media:
                        Convert<MediaContentItem>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.Collection:
                        Convert<CollectionContentItem>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.List:
                        Convert<ListContentItem>(attachment);
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
                throw new ValidationException($"Failed to convert Actions SDK Attachment with ContentType {attachment?.ContentType} to {typeof(T).Name}", ex);
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
