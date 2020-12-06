using System;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Xunit;

namespace Bot.Builder.Community.Adapters.Shared.Tests.TestUtilities
{
    public abstract class AttachmentConverterTestBase
    {
        protected abstract IAttachmentConverter GetConverter();

        protected void ConvertNullDoesNothingTest()
        {
            var converter = GetConverter();
            Assert.False(converter.ConvertAttachmentContent(null));
        }

        protected void ConvertSupportedCardTest(Attachment attachment, Type cardType)
        {
            attachment = attachment.Anonymize();
            Assert.IsNotType(cardType, attachment.Content);

            var converter = GetConverter();

            Assert.True(converter.ConvertAttachmentContent(attachment));
            Assert.IsType(cardType, attachment.Content);
        }

        protected void ConvertNotSupportedCardTest()
        {
            var attachment = new NotSupportedCard().ToAttachment().Anonymize();

            Assert.IsNotType<NotSupportedCard>(attachment.Content);

            var converter = GetConverter();

            Assert.False(converter.ConvertAttachmentContent(attachment));
            Assert.IsNotType<NotSupportedCard>(attachment.Content);
        }

        protected void BadTypeThrowsTest(string doppelgangerType)
        {
            var attachment = new NotSupportedCard().ToAttachment(); // This has a Title property of a different type from the HeroCard.
            attachment.ContentType = doppelgangerType; // mangle the content type.

            // Type remains unchanged so far.
            Assert.IsType<NotSupportedCard>(attachment.Content);

            attachment = attachment.Anonymize();

            // After anonymizing type is lost.
            Assert.IsNotType<NotSupportedCard>(attachment.Content);

            var converter = GetConverter();

            // Exception is thrown.
            Assert.Throws<ValidationException>(() => converter.ConvertAttachmentContent(attachment));
        }
    }
}
