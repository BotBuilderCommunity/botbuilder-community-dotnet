using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.ToActivity;
using Microsoft.Bot.Schema;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.Tests.ToActivityTests
{
    public class InfobipViberDeliveryReportToActivityTests
    {
        [Fact]
        public void ConvertViberDeliveryReportEventToActivity()
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

            var result = incomingMessage.Results.Single();
            var activity = InfobipViberDeliveryReportToActivity.Convert(result, TestOptions.ViberNumber);

            Assert.NotNull(activity);
            Assert.Equal(InfobipViberConstants.ChannelName, activity.ChannelId);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(InfobipReportTypes.DELIVERY, activity.Name);

            Assert.Null(activity.Text);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity, TestOptions.ViberNumber);
        }

        private void VerifyResultCoreProperties(InfobipIncomingResultBase result, Activity activity, string fromId)
        {
            Assert.Equal(result.MessageId, activity.Id);
            Assert.Equal(fromId, activity.From.Id);
            Assert.Equal(result.To, activity.Recipient.Id);
            Assert.Equal(result.To, activity.Conversation.Id);
            Assert.Equal(result, activity.ChannelData);
            Assert.Equal(result.DoneAt, activity.Timestamp);
        }
    }
}
