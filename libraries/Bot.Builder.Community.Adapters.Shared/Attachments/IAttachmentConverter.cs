using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Shared.Attachments
{
    /// <summary>
    /// Attachment converter. All methods here should be idempotent.
    /// </summary>
    public interface IAttachmentConverter
    {
        /// <summary>
        /// Convert the attachment Content property to type appropriate for it's ContentType.
        /// If the content type is not something this converter understands it should convert it and return True, otherwise it should ignore it
        /// and return False.
        /// </summary>
        /// <param name="attachment">The attachment to convert.</param>
        /// <returns>True if a conversion was attempted otherwise false.</returns>
        bool ConvertAttachmentContent(Attachment attachment);
    }
}
