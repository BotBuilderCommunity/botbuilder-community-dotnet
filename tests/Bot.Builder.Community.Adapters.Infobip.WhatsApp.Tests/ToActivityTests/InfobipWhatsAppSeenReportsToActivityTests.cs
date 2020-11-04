using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.ToActivity;
using Microsoft.Bot.Schema;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp.Tests.ToActivityTests
{
    public class InfobipWhatsAppSeenReportsToActivityTests
    {
        [Fact]
        public void ConvertSmsSeenEventToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipWhatsAppIncomingResult>
            {
                Results = new List<InfobipWhatsAppIncomingResult>
                {
                    new InfobipWhatsAppIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "whatsapp-number",
                        To = "subscriber-number",
                        SeenAt = DateTimeOffset.UtcNow,
                        SentAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(10))
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = InfobipWhatsAppSeenReportToActivity.Convert(incomingMessage.Results.Single());

            Assert.NotNull(activity);
            Assert.Equal(InfobipWhatsAppConstants.ChannelName, activity.ChannelId);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(InfobipReportTypes.SEEN, activity.Name);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
        }

        private static void VerifyResultCoreProperties(InfobipWhatsAppIncomingResult result, Activity activity)
        {
            var timestamp = result.SeenAt ?? result.DoneAt;

            Assert.Equal(result.MessageId, activity.Id);
            Assert.Equal(result.From, activity.From.Id);
            Assert.Equal(result.To, activity.Recipient.Id);
            Assert.Equal(result.To, activity.Conversation.Id);
            Assert.Equal(result, activity.ChannelData);
            Assert.Equal(timestamp, activity.Timestamp);
        }
    }
}
