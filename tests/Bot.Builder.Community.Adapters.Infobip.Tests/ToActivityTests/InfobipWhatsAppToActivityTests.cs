using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Bot.Builder.Community.Adapters.Infobip.ToActivity;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Tests.ToActivityTests
{
    public class InfobipWhatsAppToActivityTests
    {
        private Mock<IInfobipClient> _infobipClient;
        private const string _contentType = "image";

        public InfobipWhatsAppToActivityTests()
        {
            _infobipClient = new Mock<IInfobipClient>(MockBehavior.Strict);
            _infobipClient.Setup(x => x.GetContentTypeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(_contentType);
        }

        [Fact]
        public async Task ConvertWhatsAppTextMessageToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "whatsapp-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "WHATSAPP",
                        Message = new InfobipIncomingWhatsAppMessage
                        {
                            Type = InfobipIncomingMessageTypes.Text,
                            Text = "Text message to bot"
                        },
                        Contact = new InfobipIncomingWhatsAppContact
                        {
                            Name = "Whatsapp Subscriber Name"
                        },
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        },
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = (await InfobipWhatsAppToActivity.Convert(incomingMessage.Results.Single(), _infobipClient.Object).ConfigureAwait(false));

            Assert.NotNull(activity);
            Assert.Equal(InfobipChannel.WhatsApp, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultTextMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertWhatsAppImageMessageToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "whatsapp-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "WHATSAPP",
                        Message = new InfobipIncomingWhatsAppMessage
                        {
                            Caption = "Message Caption",
                            Type = InfobipIncomingMessageTypes.Image,
                            Url = new Uri("https://infobip.api.media.endpoint")
                        },
                        Contact = new InfobipIncomingWhatsAppContact
                        {
                            Name = "Whatsapp Subscriber Name"
                        },
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        },
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = await InfobipWhatsAppToActivity.Convert(incomingMessage.Results.Single(), _infobipClient.Object).ConfigureAwait(false);

            Assert.NotNull(activity);
            Assert.Equal(InfobipChannel.WhatsApp, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultImageMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertWhatsAppLocationMessageToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "whatsapp-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "WHATSAPP",
                        Message = new InfobipIncomingWhatsAppMessage
                        {
                            Type = InfobipIncomingMessageTypes.Location,
                            Caption = "Message caption",
                            Longitude = 15.9459228515625,
                            Latitude = 45.793365478515625
                        },
                        Contact = new InfobipIncomingWhatsAppContact
                        {
                            Name = "Whatsapp Subscriber Name"
                        },
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        },
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = await InfobipWhatsAppToActivity.Convert(incomingMessage.Results.Single(), _infobipClient.Object).ConfigureAwait(false);

            Assert.NotNull(activity);
            Assert.Equal(InfobipChannel.WhatsApp, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultLocationMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertWhatsAppUnsupportedMessageTypeToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "whatsapp-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "WHATSAPP",
                        Message = new InfobipIncomingWhatsAppMessage
                        {
                            Type = "UNSUPPORTED",
                        },
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activities = await InfobipWhatsAppToActivity.Convert(incomingMessage.Results.Single(), _infobipClient.Object).ConfigureAwait(false);

            Assert.Null(activities);
        }

        private void VerifyResultCoreProperties(InfobipIncomingResult result, Activity activity)
        {
            Assert.Equal(result.MessageId, activity.Id);
            Assert.Equal(result.From, activity.From.Id);
            Assert.Equal(result.To, activity.Recipient.Id);
            Assert.Equal(result.From, activity.Conversation.Id);
            Assert.Equal(result, activity.ChannelData);
            Assert.Equal(result.ReceivedAt, activity.Timestamp);
        }

        private void VerifyResultTextMessage(InfobipIncomingWhatsAppMessage message, Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(message.Text, activity.Text);
            Assert.Equal(TextFormatTypes.Plain, activity.TextFormat);

            Assert.True(activity.Attachments == null || activity.Attachments.Count == 0);
        }

        private void VerifyResultImageMessage(InfobipIncomingWhatsAppMessage message, Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);

            Assert.NotNull(activity.Attachments);
            Assert.Equal(1, activity.Attachments.Count);

            var attachment = activity.Attachments[0];

            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(message.Url.AbsoluteUri, attachment.ContentUrl);
            Assert.Equal(message.Caption, attachment.Name);
            Assert.Equal(message.Attachment, attachment.Content);
            Assert.Equal(_contentType, attachment.ContentType);

            Assert.Null(activity.Text);
        }

        private void VerifyResultLocationMessage(InfobipIncomingWhatsAppMessage message, Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);

            Assert.Equal(1, activity.Entities.Count);
            var entity = activity.Entities.First().GetAs<GeoCoordinates>();
            Assert.NotNull(entity);
            Assert.Equal(message.Longitude, entity.Longitude);
            Assert.Equal(message.Latitude, entity.Latitude);
        }
    }
}
