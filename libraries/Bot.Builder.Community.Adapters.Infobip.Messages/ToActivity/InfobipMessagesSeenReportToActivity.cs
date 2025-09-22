using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Microsoft.Bot.Schema;
using System;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.ToActivity
{
    public static class InfobipMessagesSeenReportToActivity
    {
        /// <summary>
        /// Converts Infobip seen report to Bot Framework activity
        /// </summary>
        /// <param name="response">Infobip seen report response</param>
        /// <returns>Bot Framework activity</returns>
        public static Activity Convert(InfobipMessagesIncomingResult response)
        {
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Name = "SEEN",
                Id = response.MessageId,
                Timestamp = response.SeenAt ?? DateTimeOffset.UtcNow,
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
                    Channel = response.Channel,
                    Platform = response.Platform,
                    SeenAt = response.SeenAt,
                    SentAt = response.SentAt,
                    CallbackData = response.CallbackData
                }
            };

            return activity;
        }
    }
}