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
        /// Convert the attachment Content to the type T or throw a ValidationException if that is not possible.
        /// </summary>
        /// <remarks>
        /// If this method is called then the Attachment.ContentType is assumed to be a well-known type. If conversion fails, for any reason, then we want to let the
        /// developer know there is an issue by throwing a ValidationException. If the type is not a well-known type then this method will not be called and the attachment 
        /// will not be converted (it will be ignored).
        /// </remarks>
        public static void Convert<T>(Attachment attachment)
        {
            try
            {
                attachment.Content = attachment.ContentAs<T>();
            }
            catch (Exception ex)
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

        /// <summary>
        /// Convert the Attachment Content field to the given type. An exception is thrown if the conversion fails.
        /// </summary>
        private static T ContentAs<T>(this Attachment attachment)
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
