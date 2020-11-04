using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.ToInfobip;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.Tests.ToInfobipTests
{
    public class InfobipOmniViberMessageFactoryTests
    {
        private Activity _activity;

        public InfobipOmniViberMessageFactoryTests()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.Message,
                Id = "message id",
                Timestamp = DateTimeOffset.Parse("2020-02-26T10:15:48.734+0000"),
                ChannelId = InfobipViberConstants.ChannelName,
                Conversation = new ConversationAccount { Id = "subscriber-number" },
                From = new ChannelAccount { Id = "viber-sender" },
                Recipient = new ChannelAccount { Id = "subscriber-number" }
            };
        }

        [Fact]
        public void ConvertTextActivityToOmniViberFailoverMessage()
        {
            _activity.Text = "Test text";

            var viberMessages = InfobipOmniViberMessageFactory.Create(_activity);
            Assert.NotNull(viberMessages);
            Assert.Single(viberMessages);

            var viberMessage = viberMessages.Single();

            CheckViberTextMessage(viberMessage, _activity.Text);
        }

        [Fact]
        public void ConvertViberActivityToOmniViberFailoverMessage()
        {
            var expected = new InfobipOmniViberMessage
            {
                Text = "text",
                ImageUrl = "image url",
                ButtonText = "button text",
                ButtonUrl = "button url",
                IsPromotional = true,
                TrackingData = "tracking-data",
                ValidityPeriod = 10,
                ValidityPeriodTimeUnit = InfobipViberOptions.ValidityPeriodTimeUnitTypes.Days
            };

            _activity.AddInfobipViberMessage(expected);

            var viberMessages = InfobipOmniViberMessageFactory.Create(_activity);
            Assert.NotNull(viberMessages);
            Assert.Single(viberMessages);

            var viberMessage = viberMessages.Single();

            Assert.NotNull(viberMessage);
            Assert.Equal(expected.Text, viberMessage.Text);
            Assert.Equal(expected.ButtonText, viberMessage.ButtonText);
            Assert.Equal(viberMessage.ButtonUrl, viberMessage.ButtonUrl);
            Assert.Equal(expected.ImageUrl, viberMessage.ImageUrl);
            Assert.Equal(expected.IsPromotional, viberMessage.IsPromotional);
            Assert.Equal(expected.TrackingData, viberMessage.TrackingData);
            Assert.Equal(expected.ValidityPeriodTimeUnit, viberMessage.ValidityPeriodTimeUnit);
            Assert.Equal(expected.ValidityPeriod, viberMessage.ValidityPeriod);
        }

        [Fact]
        public void ConvertViberActivityWithTextToOmniViberFailoverMessage()
        {
            _activity.Text = "Message activity text";
            var expected = new InfobipOmniViberMessage
            {
                Text = "text",
                ImageUrl = "image url",
                ButtonText = "button text",
                ButtonUrl = "button url",
                IsPromotional = true,
                TrackingData = "tracking-data",
                ValidityPeriod = 10,
                ValidityPeriodTimeUnit = InfobipViberOptions.ValidityPeriodTimeUnitTypes.Days
            };

            _activity.AddInfobipViberMessage(expected);

            var viberMessages = InfobipOmniViberMessageFactory.Create(_activity);
            Assert.NotNull(viberMessages);
            Assert.Equal(2, viberMessages.Count);

            var viberTextMessage = viberMessages.First();
            CheckViberTextMessage(viberTextMessage, _activity.Text);

            var viberMessage = viberMessages.Last();

            Assert.NotNull(viberMessage);
            Assert.Equal(expected.Text, viberMessage.Text);
            Assert.Equal(expected.ButtonText, viberMessage.ButtonText);
            Assert.Equal(viberMessage.ButtonUrl, viberMessage.ButtonUrl);
            Assert.Equal(expected.ImageUrl, viberMessage.ImageUrl);
            Assert.Equal(expected.IsPromotional, viberMessage.IsPromotional);
            Assert.Equal(expected.TrackingData, viberMessage.TrackingData);
            Assert.Equal(expected.ValidityPeriodTimeUnit, viberMessage.ValidityPeriodTimeUnit);
            Assert.Equal(expected.ValidityPeriod, viberMessage.ValidityPeriod);
        }

        [Fact]
        public void ConvertViberActivityWithoutButtonUrlToOmniViberFailoverMessage()
        {
            var expected = new InfobipOmniViberMessage
            {
                Text = "text",
                ImageUrl = "image url",
                ButtonText = "button text",
                IsPromotional = true,
                TrackingData = "tracking-data",
                ValidityPeriod = 10,
                ValidityPeriodTimeUnit = InfobipViberOptions.ValidityPeriodTimeUnitTypes.Days
            };

            _activity.AddInfobipViberMessage(expected);

            Assert.Throws<Exception>(() => InfobipOmniViberMessageFactory.Create(_activity));
        }

        [Fact]
        public void ConvertViberActivityWithoutButtonUrlTextToOmniViberFailoverMessage()
        {
            var expected = new InfobipOmniViberMessage
            {
                Text = "text",
                ImageUrl = "image url",
                ButtonUrl = "button url",
                IsPromotional = true,
                TrackingData = "tracking-data",
                ValidityPeriod = 10,
                ValidityPeriodTimeUnit = InfobipViberOptions.ValidityPeriodTimeUnitTypes.Days
            };

            _activity.AddInfobipViberMessage(expected);

            Assert.Throws<Exception>(() => InfobipOmniViberMessageFactory.Create(_activity));
        }

        private void CheckViberTextMessage(InfobipOmniViberMessage viberMessage, string text)
        {
            Assert.NotNull(viberMessage);
            Assert.Equal(viberMessage.Text, text);
            Assert.Null(viberMessage.ButtonText);
            Assert.Null(viberMessage.ButtonUrl);
            Assert.Null(viberMessage.ImageUrl);
            Assert.False(viberMessage.IsPromotional);
            Assert.Null(viberMessage.TrackingData);
            Assert.Null(viberMessage.ValidityPeriodTimeUnit);
            Assert.Equal(0, viberMessage.ValidityPeriod);
        }
    }
}
