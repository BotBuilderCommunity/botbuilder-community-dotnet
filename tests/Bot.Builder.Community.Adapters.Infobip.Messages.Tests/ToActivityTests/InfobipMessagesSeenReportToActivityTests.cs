using System;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.ToActivity;
using Microsoft.Bot.Schema;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Tests.ToActivityTests
{
    public class InfobipMessagesSeenReportToActivityTests
    {
        [Fact]
        public void ConvertMessagesSeenReportToActivity()
        {
            var seenReport = new InfobipMessagesIncomingResult
            {
                MessageId = "Unique message Id",
                From = "messages-number",
                To = "subscriber-number",
                SeenAt = DateTimeOffset.UtcNow,
                SentAt = DateTimeOffset.UtcNow.AddMinutes(-1),
                Channel = "whatsapp",
                Platform = "whatsapp"
            };

            var activity = InfobipMessagesSeenReportToActivity.Convert(seenReport);

            Assert.NotNull(activity);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal("SEEN", activity.Name);
            Assert.Equal(InfobipMessagesConstants.ChannelId, activity.ChannelId);
            Assert.Equal(seenReport.MessageId, activity.Id);
            Assert.Equal(seenReport.SeenAt, activity.Timestamp);
            Assert.Equal(seenReport.To, activity.From.Id);
            Assert.Equal(seenReport.From, activity.Recipient.Id);
            Assert.Equal(seenReport.From, activity.Conversation.Id);
            Assert.False(activity.Conversation.IsGroup);

            Assert.NotNull(activity.ChannelData);
        }
    }
}