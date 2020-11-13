using System.Collections.Generic;
using AdaptiveCards;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Bot.Builder.Community.Adapters.Shared.Tests.TestUtilities;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Xunit;

namespace Bot.Builder.Community.Adapters.Shared.Tests.Attachments
{
    public class AttachmentConverterTests
    {
        [Fact]
        public void AttachmentConverterCorrectDefaults()
        {
            var converter = new AttachmentConverter();
            Assert.Equal(2, converter.Converters.Count);
            Assert.IsType<CardAttachmentConverter>(converter.Converters[0]);
            Assert.IsType<AdaptiveCardAttachmentConverter>(converter.Converters[1]);
        }

        [Fact]
        public void AttachmentConverterCanHaveNoDefaultConverters()
        {
            var converter = new AttachmentConverter(false);
            Assert.Equal(0, converter.Converters.Count);
        }

        [Fact]
        public void ConvertAttachmentsNullDoesNothing()
        {
            var converter = new AttachmentConverter();
            converter.ConvertAttachments(null);
        }

        [Fact]
        public void ConvertAttachmentNullAttachmentsDoesNothing()
        {
            var activity = CreateActivity();
            var converter = new AttachmentConverter();
            converter.ConvertAttachments(activity);
        }

        [Fact]
        public void DefaultFailsOnFirstError()
        {
            // 1st attachment is bad (mismatched content type), 2nd attachment is fine.
            var activity = CreateActivity(new NotSupportedCard().ToAttachment(), new HeroCard().ToAttachment());
            activity.Attachments[0].ContentType = HeroCard.ContentType;

            Assert.IsType<NotSupportedCard>(activity.Attachments[0].Content);
            Assert.IsType<HeroCard>(activity.Attachments[1].Content);

            activity = activity.Anonymize();

            Assert.IsNotType<NotSupportedCard>(activity.Attachments[0].Content);
            Assert.IsNotType<HeroCard>(activity.Attachments[1].Content);

            var converter = new AttachmentConverter(useDefaults: true);
            Assert.Throws<ValidationException>(() => converter.ConvertAttachments(activity));

            // Throw stopped processing at 1st card.
            Assert.IsNotType<HeroCard>(activity.Attachments[1].Content);
        }

        [Fact]
        public void ConvertFromTwoConverters()
        {
            var activity = CreateActivity(new HeroCard().ToAttachment(), new AdaptiveCard(AdaptiveCard.KnownSchemaVersion).ToAttachment());

            Assert.IsType<HeroCard>(activity.Attachments[0].Content);
            Assert.IsType<AdaptiveCard>(activity.Attachments[1].Content);

            activity = activity.Anonymize();

            Assert.IsNotType<HeroCard>(activity.Attachments[0].Content);
            Assert.IsNotType<AdaptiveCard>(activity.Attachments[1].Content);

            var converter = new AttachmentConverter(useDefaults: true);
            converter.ConvertAttachments(activity);

            Assert.IsType<HeroCard>(activity.Attachments[0].Content);
            Assert.IsType<AdaptiveCard>(activity.Attachments[1].Content);
        }

        private Activity CreateActivity(params Attachment[] attachments)
        {
            return new Activity
            {
                Text = "text",
                Attachments = attachments == null ? null : new List<Attachment>(attachments)
            };
        }
    }
}
