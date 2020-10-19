using System;
using Bot.Builder.Community.Adapters.Infobip.Sms.Models;
using Bot.Builder.Community.Adapters.Infobip.Sms.ToInfobip;
using Microsoft.Bot.Schema;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Sms.Tests.ToInfobipTests
{
    public class InfobipOmniSmsMessageFactoryTests
    {
        private Activity _activity;

        public InfobipOmniSmsMessageFactoryTests()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.Message,
                Id = "message id",
                Timestamp = DateTimeOffset.Parse("2020-02-26T10:15:48.734+0000"),
                ChannelId = InfobipSmsConstants.ChannelName,
                Conversation = new ConversationAccount { Id = "subscriber-number" },
                From = new ChannelAccount { Id = "sms-number" },
                Recipient = new ChannelAccount { Id = "subscriber-number" }
            };
        }

        [Fact]
        public void ConvertTextActivityToOmniSmsFailoverMessageWithoutOptions()
        {
            _activity.Text = "Test text";

            var smsMessage = InfobipOmniSmsMessageFactory.Create(_activity);

            Assert.NotNull(smsMessage);
            Assert.Equal(smsMessage.Text, _activity.Text);
            Assert.Equal(0, smsMessage.ValidityPeriod);
            Assert.Null(smsMessage.ValidityPeriodTimeUnit);
            Assert.Null(smsMessage.Transliteration);
        }

        [Fact]
        public void ConvertTextActivityToOmniSmsFailoverMessageWithOptions()
        {
            _activity.Text = "Test text";

            var smsOptions = new InfobipOmniSmsMessageOptions();
            smsOptions.ValidityPeriodTimeUnit = InfobipSmsOptions.ValidityPeriodTimeUnitTypes.Hours;
            smsOptions.ValidityPeriod = 1;
            smsOptions.Transliteration = InfobipSmsOptions.TransliterationTypes.All;
            smsOptions.Language = new InfobipOmniSmsLanguage{LanguageCode = InfobipSmsOptions.LanguageCode.Autodetect};

            _activity.AddInfobipOmniSmsMessageOptions(smsOptions);

            var smsMessage = InfobipOmniSmsMessageFactory.Create(_activity);

            Assert.NotNull(smsMessage);
            Assert.Equal(smsMessage.Text, _activity.Text);
            Assert.Equal(smsMessage.ValidityPeriod, smsOptions.ValidityPeriod);
            Assert.Equal(smsMessage.ValidityPeriodTimeUnit, smsOptions.ValidityPeriodTimeUnit);
            Assert.Equal(smsMessage.Transliteration, smsOptions.Transliteration);
            Assert.NotNull(smsMessage.Language);
            Assert.Equal(smsMessage.Language.LanguageCode, smsOptions.Language.LanguageCode);
        }
    }
}
