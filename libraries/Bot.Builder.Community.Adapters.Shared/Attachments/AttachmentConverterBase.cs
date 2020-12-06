using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Shared.Attachments
{
    public abstract class AttachmentConverterBase : IAttachmentConverter
    {
        /// <summary>
        /// The name of this converter.
        /// </summary>
        protected abstract string Name { get; }

        /// <summary>
        /// The converters - dictionary of Attachment.ContentType to converter (typically Convert).
        /// </summary>
        protected abstract IReadOnlyDictionary<string, Action<Attachment>> Converters { get; }

        public bool ConvertAttachmentContent(Attachment attachment)
        {
            if (attachment?.ContentType != null && Converters.TryGetValue(attachment.ContentType, out var converter))
            {
                converter(attachment);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert the attachment Content to the type T or throw a ValidationException if that is not possible.
        /// </summary>
        /// <remarks>
        /// If this method is called then the Attachment.ContentType is assumed to be a well-known type. If conversion fails, for any reason, then we want to let the
        /// developer know there is an issue by throwing a ValidationException. If the type is not a well-known type then this method will not be called and the attachment 
        /// will not be converted (it will be ignored).
        /// </remarks>
        protected void Convert<T>(Attachment attachment)
        {
            try
            {
                attachment.Content = ContentAs<T>(attachment);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"{Name} failed to convert Attachment with ContentType {attachment?.ContentType} to {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Convert the Attachment Content field to the given type. An exception is thrown if the conversion fails.
        /// </summary>
        private static T ContentAs<T>(Attachment attachment)
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
            if (attachment.Content is T contentAsT)
            {
                return contentAsT;
            }
            if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)System.Convert.FromBase64String(attachment.Content.ToString());
            }
            if (attachment.Content is string contentAsString)
            {
                return JsonConvert.DeserializeObject<T>(contentAsString);
            }
            return (T)((JObject)attachment.Content).ToObject<T>();
        }
    }
}
