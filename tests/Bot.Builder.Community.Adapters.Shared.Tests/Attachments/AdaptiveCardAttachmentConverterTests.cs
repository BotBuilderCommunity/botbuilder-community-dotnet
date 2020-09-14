using AdaptiveCards;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Bot.Builder.Community.Adapters.Shared.Tests.TestUtilities;
using Microsoft.Bot.Schema;
using Xunit;

namespace Bot.Builder.Community.Adapters.Shared.Tests.Attachments
{
    public class AdaptiveCardAttachmentConverterTests : AttachmentConverterTestBase
    {
        protected override IAttachmentConverter GetConverter() => new AdaptiveCardAttachmentConverter();

        [Fact]
        public void ConvertNullDoesNothing() => ConvertNullDoesNothingTest();

        [Fact]
        public void ConvertAdaptiveCardCard() => ConvertSupportedCardTest(new AdaptiveCard(AdaptiveCard.KnownSchemaVersion).ToAttachment(), typeof(AdaptiveCard));

        [Fact]
        public void ConvertNotSupportedCard() => ConvertNotSupportedCardTest();

        [Fact]
        public void BadTypeThrows() => BadTypeThrowsTest(AdaptiveCard.ContentType);
    }
}
