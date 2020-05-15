using Bot.Builder.Community.Adapters.Infobip;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bot.Builder.Community.Adapter.Infobip.Tests
{
    public class ToActivityConverterTest
    {
        private const string _contentType = "image";

        private InfobipAdapterOptions _adapterOptions;
        private ToActivityConverter _toActivityConverter;

        public ToActivityConverterTest()
        {
            var infobipClient = new Mock<IInfobipClient>(MockBehavior.Strict);
            infobipClient.Setup(x => x.GetContentTypeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(_contentType);

            _adapterOptions = TestOptions.Get();
            _toActivityConverter = new ToActivityConverter(_adapterOptions, infobipClient.Object, NullLogger.Instance);
        }

        [Fact]
        public async Task ConvertTextToActivity()
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

            var activity = (await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false)).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipConstants.ChannelName, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultTextMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertSeenToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "whatsapp-number",
                        To = "subscriber-number",
                        SeenAt = DateTimeOffset.UtcNow,
                        SentAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(10))
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = (await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false)).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipConstants.ChannelName, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity, isEvent: true);
            VerifyResultEventMessage(InfobipReportTypes.SEEN, activity);
        }

        [Fact]
        public async Task ConvertDeliveryReportToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        To = "subscriber-number",
                        SentAt = DateTimeOffset.UtcNow,
                        DoneAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(10)),
                        Status = new InfobipIncomingInfoMessage
                        {
                            Id = 5,
                            GroupId = 3,
                            GroupName = "DELIVERED",
                            Name = "DELIVERED_TO_HANDSET",
                            Description = "Message delivered to handset"
                        },
                        Error = new InfobipIncomingInfoMessage
                        {
                            Id = 0,
                            GroupId = 0,
                            GroupName = "OK",
                            Name = "NO_ERROR",
                            Description = "No Error",
                            Permanent = false
                        },
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        }
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };


            var activity = (await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false)).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipConstants.ChannelName, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity, isEvent: true, overrideFromId: _adapterOptions.InfobipWhatsAppNumber);
            VerifyResultEventMessage(InfobipReportTypes.DELIVERY, activity);
        }

        [Fact]
        public async Task ConvertImageToActivity()
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

            var activity = (await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false)).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipConstants.ChannelName, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultImageMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertLocationToActivity()
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

            var activity = (await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false)).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipConstants.ChannelName, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultLocationMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertUnsupportedMesageTypeToActivity()
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

            var activities = (await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false)).ToList();

            Assert.NotNull(activities);
            Assert.Empty(activities);
        }

        [Fact]
        public async Task ConvertTextWithCallbackDataToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        To = "subscriber-number",
                        SentAt = DateTimeOffset.UtcNow,
                        DoneAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(10)),
                        Status = new InfobipIncomingInfoMessage
                        {
                            Id = 5,
                            GroupId = 3,
                            GroupName = "DELIVERED",
                            Name = "DELIVERED_TO_HANDSET",
                            Description = "Message delivered to handset"
                        },
                        Error = new InfobipIncomingInfoMessage
                        {
                            Id = 0,
                            GroupId = 0,
                            GroupName = "OK",
                            Name = "NO_ERROR",
                            Description = "No Error",
                            Permanent = false
                        },
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        },
                        CallbackData = "{\"initialMenu\": true, \"userId\": 1, \"username\":\"newUser\"}",
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = (await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false)).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipConstants.ChannelName, activity.ChannelId);
            var entity = activity.Entities.Single(x => x.Type == InfobipConstants.InfobipCallbackDataEntityType);
            var result = entity.Properties.ToDictionary();

            Assert.Equal("true", result["initialMenu"]);
            Assert.Equal("1", result["userId"]);
            Assert.Equal("newUser", result["username"]);
            VerifyResultCoreProperties(incomingMessage.Results[0], activity, isEvent: true, overrideFromId: _adapterOptions.InfobipWhatsAppNumber);
            VerifyResultEventMessage(InfobipReportTypes.DELIVERY, activity);
        }

        private void VerifyResultCoreProperties(InfobipIncomingResult result, Activity activity, bool isEvent = false, string overrideFromId = null)
        {
            var conversationId = isEvent ? result.To : result.From;
            var timestamp = isEvent ? (result.SeenAt ?? result.DoneAt) : result.ReceivedAt;

            Assert.Equal(result.MessageId, activity.Id);
            Assert.Equal(overrideFromId ?? result.From, activity.From.Id);
            Assert.Equal(result.To, activity.Recipient.Id);
            Assert.Equal(conversationId, activity.Conversation.Id);
            Assert.Equal(result, activity.ChannelData);
            Assert.Equal(timestamp, activity.Timestamp);
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
            Assert.Equal(message.Url.ToString(), attachment.ContentUrl);
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

        private void VerifyResultEventMessage(string type, Activity activity)
        {
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(type, activity.Name);

            Assert.Null(activity.Text);
        }
    }
}