using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Sms.Models;
using Bot.Builder.Community.Adapters.Infobip.Sms.ToActivity;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Sms.Tests.ToActivityTests
{
    public class ToSmsActivityConverterTest
    {
        private InfobipSmsAdapterOptions _adapterOptions;
        private ToSmsActivityConverter _toSmsActivityConverter;

        public ToSmsActivityConverterTest()
        {

            _adapterOptions = TestOptions.Get();
            _toSmsActivityConverter = new ToSmsActivityConverter(_adapterOptions, NullLogger.Instance);
        }

        [Fact]
        public void ConvertSmsMessageWithCallbackDataToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipSmsIncomingResult>
            {
                Results = new List<InfobipSmsIncomingResult>
                {
                    new InfobipSmsIncomingResult
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

            var activity = _toSmsActivityConverter.Convert(incomingMessage).First();

            Assert.NotNull(activity);
            Assert.Equal(InfobipSmsConstants.ChannelName, activity.ChannelId);
            var entity = activity.Entities.Single(x => x.Type == InfobipEntityType.CallbackData);
            var result = entity.Properties.ToDictionary();

            Assert.Equal("true", result["initialMenu"]);
            Assert.Equal("1", result["userId"]);
            Assert.Equal("newUser", result["username"]);
        }
    }
}