using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Bot.Builder.Community.Adapters.Infobip.ToActivity;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Tests.ToActivityTests
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
        public async Task ConvertWhatsAppTextMessageWithCallbackDataToActivity()
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
                        CallbackData = "{\"initialMenu\": true, \"userId\": 1, \"username\":\"newUser\"}"
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = (await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false)).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipChannel.WhatsApp, activity.ChannelId);
            var entity = activity.Entities.Single(x => x.Type == InfobipEntityType.CallbackData);
            var result = entity.Properties.ToDictionary();

            Assert.Equal("true",result["initialMenu"]);
            Assert.Equal("1", result["userId"]);
            Assert.Equal("newUser", result["username"]);
        }

        [Fact]
        public async Task ConvertSmsMessageWithCallbackDataToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "sms-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        },
                        CallbackData = "{\"initialMenu\": true, \"userId\": 1, \"username\":\"newUser\"}",
                        SmsCount = 1,
                        Text = "Keyword text",
                        CleanText = "Text"
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = (await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false)).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipChannel.Sms, activity.ChannelId);
            var entity = activity.Entities.Single(x => x.Type == InfobipEntityType.CallbackData);
            var result = entity.Properties.ToDictionary();

            Assert.Equal("true", result["initialMenu"]);
            Assert.Equal("1", result["userId"]);
            Assert.Equal("newUser", result["username"]);
        }

        [Fact]
        public async Task ConvertWhatsAppDeliveryReportEventToActivity()
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
                        Channel = "WHATSAPP",
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
            Assert.Equal(InfobipChannel.WhatsApp, activity.ChannelId);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(InfobipReportTypes.DELIVERY, activity.Name);
        }

        [Fact]
        public async Task ConvertSmsSeenEventToActivity()
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
            Assert.Equal(InfobipChannel.WhatsApp, activity.ChannelId);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(InfobipReportTypes.SEEN, activity.Name);
        }
    }
}