using Bot.Builder.Community.Adapters.Infobip.Models;
using Bot.Builder.Community.Adapters.Infobip.ToActivity;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Tests.ToActivityTests
{
    public class InfobipSmsToActivityTests
    {
        [Fact]
        public void ConvertSmsMessageToActivity()
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

            var activity = InfobipSmsToActivity.Convert(incomingMessage.Results.Single());

            Assert.NotNull(activity);
            Assert.Equal(InfobipChannel.Sms, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(incomingMessage.Results[0].CleanText, activity.Text);
            Assert.Equal(TextFormatTypes.Plain, activity.TextFormat);

            Assert.True(activity.Attachments == null || activity.Attachments.Count == 0);
        }

        [Fact]
        public void ConvertSmsMessageWithCallbackDataToActivity()
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

            var activity = InfobipSmsToActivity.Convert(incomingMessage.Results.Single());

            Assert.NotNull(activity);
            Assert.Equal(InfobipChannel.Sms, activity.ChannelId);
            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(incomingMessage.Results[0].CleanText, activity.Text);
            Assert.Equal(TextFormatTypes.Plain, activity.TextFormat);

            Assert.True(activity.Attachments == null || activity.Attachments.Count == 0);
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
    }
}
