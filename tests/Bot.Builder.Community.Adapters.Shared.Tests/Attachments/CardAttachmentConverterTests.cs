using Bot.Builder.Community.Adapters.Shared.Attachments;
using Bot.Builder.Community.Adapters.Shared.Tests.TestUtilities;
using Microsoft.Bot.Schema;
using Xunit;

namespace Bot.Builder.Community.Adapters.Shared.Tests.Attachments
{
    public class CardAttachmentConverterTests : AttachmentConverterTestBase
    {
        protected override IAttachmentConverter GetConverter() => new CardAttachmentConverter();

        [Fact]
        public void ConvertNullDoesNothing() => ConvertNullDoesNothingTest();

        [Fact]
        public void ConvertAnimationCard() => ConvertSupportedCardTest(new AnimationCard().ToAttachment(), typeof(AnimationCard));

        [Fact]
        public void ConvertAudioCard() => ConvertSupportedCardTest(new AudioCard().ToAttachment(), typeof(AudioCard));

        [Fact]
        public void ConvertHeroCard() => ConvertSupportedCardTest(new HeroCard().ToAttachment(), typeof(HeroCard));

        [Fact]
        public void ConvertOAuthCard() => ConvertSupportedCardTest(new OAuthCard().ToAttachment(), typeof(OAuthCard));

        [Fact]
        public void ConvertReceiptCard() => ConvertSupportedCardTest(new ReceiptCard().ToAttachment(), typeof(ReceiptCard));

        [Fact]
        public void ConvertSigninCard() => ConvertSupportedCardTest(new SigninCard().ToAttachment(), typeof(SigninCard));

        [Fact]
        public void ConvertThumbnailCard() => ConvertSupportedCardTest(new ThumbnailCard().ToAttachment(), typeof(ThumbnailCard));

        [Fact]
        public void ConvertVideoCard() => ConvertSupportedCardTest(new VideoCard().ToAttachment(), typeof(VideoCard));

        [Fact]
        public void ConvertNotSupportedCard() => ConvertNotSupportedCardTest();

        [Fact]
        public void BadTypeThrows() => BadTypeThrowsTest(HeroCard.ContentType);
    }
}
