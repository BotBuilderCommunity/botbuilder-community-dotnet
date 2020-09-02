using System;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Bot.Builder.Community.Adapters.Shared.Tests.TestUtilities;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Xunit;

namespace Bot.Builder.Community.Adapters.Shared.Tests
{
    public class CardAttachmentConverterTests
    {
        [Fact]
        public void ConvertNullDoesNothing()
        {
            var converter = new CardAttachmentConverter();
            Assert.False(converter.ConvertAttachmentContent(null));
        }

        [Fact]
        public void ConvertAnimationCard() => ConvertSupportedCard(new AnimationCard().ToAttachment(), typeof(AnimationCard));

        [Fact]
        public void ConvertAudioCard() => ConvertSupportedCard(new AudioCard().ToAttachment(), typeof(AudioCard));

        [Fact]
        public void ConvertHeroCard() => ConvertSupportedCard(new HeroCard().ToAttachment(), typeof(HeroCard));

        [Fact]
        public void ConvertOAuthCard() => ConvertSupportedCard(new OAuthCard().ToAttachment(), typeof(OAuthCard));

        [Fact]
        public void ConvertReceiptCard() => ConvertSupportedCard(new ReceiptCard().ToAttachment(), typeof(ReceiptCard));

        [Fact]
        public void ConvertSigninCard() => ConvertSupportedCard(new SigninCard().ToAttachment(), typeof(SigninCard));

        [Fact]
        public void ConvertThumbnailCard() => ConvertSupportedCard(new ThumbnailCard().ToAttachment(), typeof(ThumbnailCard));

        [Fact]
        public void ConvertVideoCard() => ConvertSupportedCard(new VideoCard().ToAttachment(), typeof(VideoCard));

        [Fact]
        public void ConvertNotSupportedCard()
        {
            var attachment = new NotSupportedCard().ToAttachment().Anonymize();

            Assert.IsNotType<NotSupportedCard>(attachment.Content);

            var converter = new CardAttachmentConverter();
            Assert.False(converter.ConvertAttachmentContent(attachment));
            Assert.IsNotType<NotSupportedCard>(attachment.Content);
        }

        [Fact]
        public void BadTypeThrows()
        {
            var attachment = new NotSupportedCard().ToAttachment(); // This has a Title property of a different type from the HeroCard.
            attachment.ContentType = HeroCard.ContentType; // mangle the content type.

            // Type remains unchanged so far.
            Assert.IsType<NotSupportedCard>(attachment.Content);

            attachment = attachment.Anonymize();

            // After anonymizing type is lost.
            Assert.IsNotType<NotSupportedCard>(attachment.Content);

            var converter = new CardAttachmentConverter();

            // Exception is thrown.
            Assert.Throws<ValidationException>(() => converter.ConvertAttachmentContent(attachment));
        }

        private void ConvertSupportedCard(Attachment attachment, Type cardType)
        {
            attachment = attachment.Anonymize();
            Assert.IsNotType(cardType, attachment.Content);

            var converter = new CardAttachmentConverter();
            Assert.True(converter.ConvertAttachmentContent(attachment));
            Assert.IsType(cardType, attachment.Content);
        }
    }
}
