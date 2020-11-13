using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.ToActivity;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.Tests.ToActivityTests
{
    public class InfobipViberToActivityTests
    {
        [Fact]
        public void ConvertViberMessageToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipViberIncomingResult>
            {
                Results = new List<InfobipViberIncomingResult>
                {
                    new InfobipViberIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "viber-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        },
                        CallbackData = "{\"initialMenu\": true, \"userId\": 1, \"username\":\"newUser\"}",
                        Message = new InfobipViberIncomingMessage
                        {
                            Text = "Lorem ipsum"
                        }
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = InfobipViberToActivity.Convert(incomingMessage.Results.Single());

            Assert.NotNull(activity);
            Assert.Equal(InfobipViberConstants.ChannelName, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(incomingMessage.Results[0].Message.Text, activity.Text);
            Assert.Equal(TextFormatTypes.Plain, activity.TextFormat);

            Assert.True(activity.Attachments == null || activity.Attachments.Count == 0);
        }

        private void VerifyResultCoreProperties(InfobipViberIncomingResult result, Activity activity)
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
