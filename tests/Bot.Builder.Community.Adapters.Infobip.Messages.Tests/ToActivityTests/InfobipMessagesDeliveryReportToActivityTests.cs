using System;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.ToActivity;
using Microsoft.Bot.Schema;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Tests.ToActivityTests
{
    public class InfobipMessagesDeliveryReportToActivityTests
    {
        [Fact]
        public void ConvertMessagesDeliveryReportToActivity()
        {
            var deliveryReport = new InfobipMessagesIncomingResult
            {
                MessageId = "Unique message Id",
                From = "messages-number",
                To = "subscriber-number",
                DoneAt = DateTimeOffset.UtcNow,
                SentAt = DateTimeOffset.UtcNow.AddMinutes(-1),
                Channel = "whatsapp",
                Platform = "whatsapp",
                Status = new InfobipIncomingInfoMessage
                {
                    Id = 5,
                    Name = "DELIVERED_TO_HANDSET",
                    Description = "Message delivered to handset",
                    GroupId = 3,
                    GroupName = "DELIVERED"
                },
                Price = new InfobipIncomingPrice
                {
                    PricePerMessage = 0,
                    Currency = "GBP"
                }
            };

            var activity = InfobipMessagesDeliveryReportToActivity.Convert(deliveryReport);

            Assert.NotNull(activity);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal("DELIVERY", activity.Name);
            Assert.Equal(InfobipMessagesConstants.ChannelId, activity.ChannelId);
            Assert.Equal(deliveryReport.MessageId, activity.Id);
            Assert.Equal(deliveryReport.DoneAt, activity.Timestamp);
            Assert.Equal(deliveryReport.To, activity.From.Id);
            Assert.Equal(deliveryReport.From, activity.Recipient.Id);
            Assert.Equal(deliveryReport.From, activity.Conversation.Id);
            Assert.False(activity.Conversation.IsGroup);

            Assert.NotNull(activity.ChannelData);
        }
    }
}