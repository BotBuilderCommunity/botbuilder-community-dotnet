using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Microsoft.Bot.Schema;
using System;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.ToActivity
{
    public static class InfobipMessagesDeliveryReportToActivity
    {
        /// <summary>
        /// Converts Infobip delivery report to Bot Framework activity
        /// </summary>
        /// <param name="response">Infobip delivery report response</param>
        /// <returns>Bot Framework activity</returns>
        public static Activity Convert(InfobipMessagesIncomingResult response)
        {
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Name = "DELIVERY",
                Id = response.MessageId,
                Timestamp = response.DoneAt ?? DateTimeOffset.UtcNow,
                ChannelId = InfobipMessagesConstants.ChannelId,
                From = new ChannelAccount
                {
                    Id = response.To
                },
                Recipient = new ChannelAccount
                {
                    Id = response.From
                },
                Conversation = new ConversationAccount
                {
                    Id = response.From,
                    IsGroup = false
                },
                ChannelData = new
                {
                    MessageId = response.MessageId,
                    Status = response.Status,
                    Channel = response.Channel,
                    Platform = response.Platform,
                    DoneAt = response.DoneAt,
                    SentAt = response.SentAt,
                    Price = response.Price,
                    Error = response.Error,
                    CallbackData = response.CallbackData
                }
            };

            return activity;
        }
    }
}