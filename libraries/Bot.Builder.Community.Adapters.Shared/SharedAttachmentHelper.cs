using System;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Shared
{
    public static class SharedAttachmentHelper
    {
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

        public static void Convert<T>(Attachment attachment)
        {
            try
            {
                attachment.Content = attachment.ContentAs<T>();
            }
            catch (JsonException ex)
            {
                throw new ValidationException($"Failed to convert Attachment with ContentType {attachment?.ContentType} to {typeof(T).Name}", ex);
            }
        }

        public static Attachment CreateAttachment<T>(T card, string contentType)
        {
            return new Attachment
            {
                Content = JObject.FromObject(card, new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore }),
                ContentType = contentType,
            };
        }
    }
}
