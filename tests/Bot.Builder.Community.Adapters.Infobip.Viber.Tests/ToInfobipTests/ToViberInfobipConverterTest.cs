using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.ToInfobip;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.Tests.ToInfobipTests
{
    public class ToViberInfobipConverterTest
    {
        private Activity _activity;

        public ToViberInfobipConverterTest()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.Message,
                Id = "message id",
                Timestamp = DateTimeOffset.Parse("2020-02-26T10:15:48.734+0000"),
                Conversation = new ConversationAccount { Id = "subscriber-number" },
                From = new ChannelAccount { Id = "whatsapp-number" },
                Recipient = new ChannelAccount { Id = "subscriber-number" }
            };
        }

        [Fact]
        public void ConvertEmptyActivityToOmniFailoverMessage()
        {
            Activity activity = null;

            Assert.Throws<ArgumentNullException>(() => ToViberInfobipConverter.Convert(activity, TestOptions.ViberScenarioKey));
        }

        [Fact]
        public void ConvertTextActivityWithCallbackData_Success()
        {
            var callbackData = new Dictionary<string, string>
            {
                {"BoolProperty", "true"},
                {"NumberProperty", "12"},
                {"StringProperty", "string"},
                {"DateProperty", DateTimeOffset.MinValue.ToString()}
            };

            _activity.Text = "Activity with callback data";
            _activity.AddInfobipCallbackData(callbackData);

            var message = ToViberInfobipConverter.Convert(_activity, TestOptions.ViberScenarioKey).Single();
            Assert.Equal(message.CallbackData, JsonConvert.SerializeObject(callbackData));
            CheckDestinations(message.Destinations);
        }

        [Fact]
        public void ConvertEmptyActivityWithCallbackData_Success()
        {
            var callbackData = new Dictionary<string, string>
            {
                {"BoolProperty", "true"},
                {"NumberProperty", "12"},
                {"StringProperty", "string"},
                {"DateProperty", DateTimeOffset.MinValue.ToString()}
            };

            _activity.AddInfobipCallbackData(callbackData);

            var messages = ToViberInfobipConverter.Convert(_activity, TestOptions.ViberScenarioKey);
            Assert.Empty(messages);
        }

        [Fact]
        public void ConvertActivityWithRecipient_ThrowException()
        {
            _activity.Recipient = null;

            Assert.Throws<ValidationException>(() => ToViberInfobipConverter.Convert(_activity, TestOptions.ViberScenarioKey));
        }

        [Fact]
        public void ConvertActivityWithEmptyChannelIdToOmniViberFailoverMessage()
        {
            _activity.Text = "Test text";
            _activity.ChannelId = null;

            var omniFailoverMessages = ToViberInfobipConverter.Convert(_activity, TestOptions.ViberScenarioKey);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, TestOptions.ViberScenarioKey);
            CheckDestinations(omniFailoverMessage.Destinations);

            Assert.NotNull(omniFailoverMessage);
            Assert.NotNull(omniFailoverMessage.Viber);
        }

        [Fact]
        public void ConvertActivityWithWhatsAppChannelIdToOmniViberFailoverMessage()
        {
            _activity.Text = "Test text";
            _activity.ChannelId = InfobipViberConstants.ChannelName;

            var omniFailoverMessages = ToViberInfobipConverter.Convert(_activity, TestOptions.ViberScenarioKey);
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, TestOptions.ViberScenarioKey);
            CheckDestinations(omniFailoverMessage.Destinations);

            Assert.NotNull(omniFailoverMessage);
            Assert.NotNull(omniFailoverMessage.Viber);
            Assert.Equal(TestOptions.ViberScenarioKey, omniFailoverMessage.ScenarioKey);
            Assert.Null(omniFailoverMessage.CallbackData);
            CheckDestinations(omniFailoverMessage.Destinations);
        }

        private void CheckDestinations(InfobipDestination[] destinations)
        {
            Assert.NotNull(destinations);
            Assert.Single(destinations);
            
            var destination = destinations.First();
            Assert.NotNull(destination.To);
            Assert.Equal(destination.To.PhoneNumber, _activity.Recipient.Id);
        }
    }
}
