using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Attachments;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Mime;
using Xunit;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Tests
{
    public class GoogleBusinessMessagingRequestMapperTests
    {
        private const string ImageUrl = "https://test.com/pitcure.jpg";
        private const string ImageTitle = "TestImage";
        private const string ImageDescription = "TestImageDescription";

        [Fact]
        public void ConstructorWithNoArgumentsShouldSucceed()
        {
            Assert.NotNull(new GoogleBusinessMessagingRequestMapper());
        }

        [Fact]
        public void ConstructorWithOptionsOnlyShouldSucceed()
        {
            Assert.NotNull(new GoogleBusinessMessagingRequestMapper(new Mock<GoogleBusinessMessagingRequestMapperOptions>().Object));
        }

        [Fact]
        public void ConstructorWithLoggerOnlyShouldSucceed()
        {
            Assert.NotNull(new GoogleBusinessMessagingRequestMapper(logger: new LoggerFactory().CreateLogger("test")));
        }

        [Fact]
        public void ActivityToMessageMessage()
        {
            var replyText = "Hello world!";
            var activity = MessageFactory.Text(replyText, replyText);
            activity.Conversation = new ConversationAccount() { Id = Guid.NewGuid().ToString() };

            var googleMessage = new GoogleBusinessMessagingRequestMapper().ActivityToMessage(activity);

            Assert.Equal(googleMessage.Text, replyText);
            Assert.Equal(googleMessage.Fallback, replyText);
            Assert.Null(googleMessage.Image);
            Assert.Null(googleMessage.RichCard);
        }

        [Fact]
        public void ActivityToMessageImage()
        {
            var activity = (Activity) MessageFactory.ContentUrl(ImageUrl, MediaTypeNames.Image.Jpeg, ImageTitle, ImageTitle, ImageTitle);
            activity.Attachments[0].ContentType = GoogleAttachmentContentTypes.Image;
            activity.Conversation = new ConversationAccount() { Id = Guid.NewGuid().ToString() };

            var googleMessage = new GoogleBusinessMessagingRequestMapper().ActivityToMessage(activity);

            Assert.Null(googleMessage.Text);
            Assert.Equal(googleMessage.Fallback, ImageTitle);
            Assert.NotNull(googleMessage.Image);
            Assert.Equal(ImageUrl, googleMessage.Image.ContentInfo.FileUrl);
            Assert.Equal(ImageTitle, googleMessage.Image.ContentInfo.AltText);
            Assert.Null(googleMessage.RichCard);
        }

        [Fact]
        public void ActivityToMessageRichCard()
        {
            var activity = (Activity) MessageFactory.Attachment(MakeRichCard());
            activity.Conversation = new ConversationAccount() { Id = Guid.NewGuid().ToString() };

            var googleMessage = new GoogleBusinessMessagingRequestMapper().ActivityToMessage(activity);

            Assert.Null(googleMessage.Text);
            Assert.Null(googleMessage.Fallback);
            Assert.Null(googleMessage.Image);
            Assert.Equal(ImageDescription, googleMessage.RichCard?.StandaloneCard?.CardContent?.Description);
            Assert.Equal(ImageUrl, googleMessage.RichCard?.StandaloneCard?.CardContent?.Media?.ContentInfo?.FileUrl);
            Assert.Equal(ImageTitle, googleMessage.RichCard?.StandaloneCard?.CardContent?.Media?.ContentInfo?.AltText);
            Assert.Equal(ImageTitle, googleMessage.RichCard?.StandaloneCard?.CardContent?.Title);
        }

        [Fact]
        public void ActivityToMessageCardCarousel()
        {
            var activity = (Activity)MessageFactory.Attachment(MakeCardCarousel());
            activity.Conversation = new ConversationAccount() { Id = Guid.NewGuid().ToString() };

            var googleMessage = new GoogleBusinessMessagingRequestMapper().ActivityToMessage(activity);

            Assert.Null(googleMessage.Text);
            Assert.Null(googleMessage.Fallback);
            Assert.Null(googleMessage.Image);
            Assert.Contains(ImageDescription, googleMessage.RichCard?.CarouselCard?.CardContents?[0].Description);
            Assert.Contains(ImageUrl, googleMessage.RichCard?.CarouselCard?.CardContents?[0].Media?.ContentInfo?.FileUrl);
            Assert.Contains(ImageTitle, googleMessage.RichCard?.CarouselCard?.CardContents?[0].Media?.ContentInfo?.AltText);
            Assert.Contains(ImageTitle, googleMessage.RichCard?.CarouselCard?.CardContents?[0].Title);
            Assert.Contains(ImageDescription, googleMessage.RichCard?.CarouselCard?.CardContents?[1].Description);
            Assert.Contains(ImageUrl, googleMessage.RichCard?.CarouselCard?.CardContents?[1].Media?.ContentInfo?.FileUrl);
            Assert.Contains(ImageTitle, googleMessage.RichCard?.CarouselCard?.CardContents?[1].Media?.ContentInfo?.AltText);
            Assert.Contains(ImageTitle, googleMessage.RichCard?.CarouselCard?.CardContents?[1].Title);
        }

        private CardContent MakeCardContent()
        {
            return new CardContent()
            {
                Title = ImageTitle,
                Description = ImageDescription,
                Media = new Media()
                {
                    ContentInfo = new ContentInfo()
                    {
                        FileUrl = ImageUrl,
                        AltText = ImageTitle
                    }
                }
            };
        }

        private Attachment MakeRichCard()
        {
            var richCardContent = new RichCardContent()
            {
                StandaloneCard = new StandaloneCard()
                {
                    CardContent = MakeCardContent()
                }
            };

            return richCardContent.ToAttachment();
        }

        private Attachment MakeCardCarousel()
        {
            var carouselCardContent = new RichCardContent()
            {
                CarouselCard = new CarouselCard()
                {
                    CardWidth = CardWidth.MEDIUM,
                    CardContents = new CardContent[] { MakeCardContent(), MakeCardContent() }
                }
            };

            return carouselCardContent.ToAttachment();
        }
    }
}