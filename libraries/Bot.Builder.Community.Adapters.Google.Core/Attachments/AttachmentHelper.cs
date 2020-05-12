using System;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BasicCard = Bot.Builder.Community.Adapters.Google.Core.Model.Response.BasicCard;

namespace Bot.Builder.Community.Adapters.Google.Core.Attachments
{
    public static class AttachmentHelper
    {
        /// <summary>
        /// Convert all Google specific attachments to their correct type.
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
                    case GoogleAttachmentContentTypes.BasicCard:
                        Convert<BasicCard>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.MediaResponse:
                        Convert<MediaResponse>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.TableCard:
                        Convert<TableCard>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.CarouselIntent:
                        Convert<CarouselIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.ListIntent:
                        Convert<ListIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.DateTimeIntent:
                        Convert<DateTimeIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.PermissionsIntent:
                        Convert<PermissionsIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.PlaceLocationIntent:
                        Convert<PlaceLocationIntent>(attachment);
                        break;
                    case GoogleAttachmentContentTypes.ConfirmationIntent:
                        Convert<ConfirmationIntent>(attachment);
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
                throw new ValidationException($"Failed to convert Google Attachment with ContentType {attachment?.ContentType} to {typeof(T).Name}", ex);
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
