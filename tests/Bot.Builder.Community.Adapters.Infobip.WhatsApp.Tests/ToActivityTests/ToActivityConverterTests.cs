using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.ToActivity;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp.Tests.ToActivityTests
{
    public class ToWhatsAppActivityConverterTest
    {
        private const string _contentType = "image";

        private InfobipWhatsAppAdapterOptions _adapterOptions;
        private ToWhatsAppActivityConverter _toActivityConverter;

        public ToWhatsAppActivityConverterTest()
        {
            var infobipClient = new Mock<IInfobipWhatsAppClient>(MockBehavior.Strict);
            infobipClient.Setup(x => x.GetContentTypeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(_contentType);

            _adapterOptions = TestOptions.Get();
            _toActivityConverter = new ToWhatsAppActivityConverter(_adapterOptions, infobipClient.Object, NullLogger.Instance);
        }

        [Fact]
        public async Task ConvertWhatsAppTextMessageWithCallbackDataToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipWhatsAppIncomingResult>
            {
                Results = new List<InfobipWhatsAppIncomingResult>
                {
                    new InfobipWhatsAppIncomingResult
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
            Assert.Equal(InfobipWhatsAppConstants.ChannelName, activity.ChannelId);
            var entity = activity.Entities.Single(x => x.Type == InfobipEntityType.CallbackData);
            var result = entity.Properties.ToDictionary();

            Assert.Equal("true",result["initialMenu"]);
            Assert.Equal("1", result["userId"]);
            Assert.Equal("newUser", result["username"]);
        }

        [Fact]
        public async Task ConvertWhatsAppDeliveryReportEventToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipWhatsAppIncomingResult>
            {
                Results = new List<InfobipWhatsAppIncomingResult>
                {
                    new InfobipWhatsAppIncomingResult
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
            Assert.Equal(InfobipWhatsAppConstants.ChannelName, activity.ChannelId);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(InfobipReportTypes.DELIVERY, activity.Name);
        }
    }
}