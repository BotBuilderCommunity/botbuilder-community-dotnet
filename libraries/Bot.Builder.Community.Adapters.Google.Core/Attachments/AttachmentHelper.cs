using System;
using System.Collections.Generic;
using System.Text;
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
            // Alexa-skills-dotnet has a custom JsonConverter that converts ICards and IDirective to their correct type so we don't need to do that.
            // However, it throws in two main cases:
            //  1) [JsonException] When the json fails to deserialize due to various reasons. In this case we want to throw a validation exception to
            //     let the bot developer know something is wrong.
            //  2) [Exception] When it doesn't recognize the type. In this case we want to leave the attachment unconverted - we ignore it.
            //
            //  See: https://github.com/timheuer/alexa-skills-dotnet/blob/master/Alexa.NET/Response/Converters/CardConverter.cs
            //       https://github.com/timheuer/alexa-skills-dotnet/blob/master/Alexa.NET/Response/Converters/DirectiveConverter.cs
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
