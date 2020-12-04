using Bot.Builder.Community.Adapters.Infobip.Models;
using Bot.Builder.Community.Adapters.Infobip.ToActivity;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Tests.ToActivityTests
{
    public class InfobipDeliveryReportToActivityTests
    {
        [Fact]
        public void ConvertWhatsAppDeliveryReportEventToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        To = "subscriber-number",
                        SentAt = DateTimeOffset.UtcNow,
                        DoneAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(10)),
                        Channel = "WHATSAPP",
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
            var activity = InfobipDeliveryReportToActivity.Convert(result, TestOptions.Get());

            Assert.NotNull(activity);
            Assert.Equal(InfobipChannel.WhatsApp, activity.ChannelId);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(InfobipReportTypes.DELIVERY, activity.Name);

            Assert.Null(activity.Text);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity, TestOptions.WhatsAppNumber);
        }

        [Fact]
        public void ConvertSmsDeliveryReportEventToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
                    {
                        MessageId = "Unique message Id",
                        To = "subscriber-number",
                        SentAt = DateTimeOffset.UtcNow,
                        DoneAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(10)),
                        Channel = "SMS",
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
            var activity = InfobipDeliveryReportToActivity.Convert(result, TestOptions.Get());

            Assert.NotNull(activity);
            Assert.Equal(InfobipChannel.Sms, activity.ChannelId);
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(InfobipReportTypes.DELIVERY, activity.Name);

            Assert.Null(activity.Text);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity, TestOptions.SmsNumber);
        }

        private void VerifyResultCoreProperties(InfobipIncomingResult result, Activity activity, string fromId)
        {
            var timestamp = result.SeenAt ?? result.DoneAt;

            Assert.Equal(result.MessageId, activity.Id);
            Assert.Equal(fromId, activity.From.Id);
            Assert.Equal(result.To, activity.Recipient.Id);
            Assert.Equal(result.To, activity.Conversation.Id);
            Assert.Equal(result, activity.ChannelData);
            Assert.Equal(timestamp, activity.Timestamp);
        }
    }
}
