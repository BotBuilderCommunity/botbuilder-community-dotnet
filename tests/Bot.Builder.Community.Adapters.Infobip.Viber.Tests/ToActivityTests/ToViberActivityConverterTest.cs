using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.ToActivity;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.Tests.ToActivityTests
{
    public class ToViberActivityConverterTest
    {
        private InfobipViberAdapterOptions _adapterOptions;
        private ToViberActivityConverter _toActivityConverter;

        public ToViberActivityConverterTest()
        {
            _adapterOptions = TestOptions.Get();
            _toActivityConverter = new ToViberActivityConverter(_adapterOptions, NullLogger.Instance);
        }

        [Fact]
        public void ConvertViberTextMessageWithCallbackDataToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipViberIncomingResult>
            {
                Results = new List<InfobipViberIncomingResult>
                {
                    new InfobipViberIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "viber-sender",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "VIBER",
                        Message = new InfobipViberIncomingMessage
                        {
                            Text = "Just simple text"
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

            var activity = _toActivityConverter.Convert(incomingMessage).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipViberConstants.ChannelName, activity.ChannelId);
            var entity = activity.Entities.Single(x => x.Type == InfobipEntityType.CallbackData);
            var result = entity.Properties.ToDictionary();

            Assert.Equal("true",result["initialMenu"]);
            Assert.Equal("1", result["userId"]);
            Assert.Equal("newUser", result["username"]);
        }

        [Fact]
        public void ConvertWhatsAppDeliveryReportEventToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipViberIncomingResult>
            {
                Results = new List<InfobipViberIncomingResult>
                {
                    new InfobipViberIncomingResult
                    {
                        MessageId = "Unique message Id",
                        To = "subscriber-number",
                        SentAt = DateTimeOffset.UtcNow,
                        DoneAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(10)),
                        Channel = "VIBER",
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

            var activity = _toActivityConverter.Convert(incomingMessage).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipViberConstants.ChannelName, activity.ChannelId);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(InfobipReportTypes.DELIVERY, activity.Name);
        }
    }
}