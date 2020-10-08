using System.Collections.Generic;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.ToActivity
{
    public abstract class InfobipBaseConverter
    {
        protected static Activity ConvertToMessage(InfobipIncomingResult result)
        {
            var activity = Convert(result);

            activity.Type = ActivityTypes.Message;
            activity.From = new ChannelAccount { Id = result.From };
            activity.Conversation = new ConversationAccount { IsGroup = false, Id = result.From };
            activity.Timestamp = result.ReceivedAt;

            return activity;
        }

        protected static Activity ConvertToEvent(InfobipIncomingResult result)
        {
            var activity = Convert(result);
            activity.Type = ActivityTypes.Event;
            activity.Conversation = new ConversationAccount { Id = result.To };
            activity.Text = null;

            return activity;
        }

        private static Activity Convert(InfobipIncomingResult result)
        {
            return new Activity
            {
                Id = result.MessageId,
                Recipient = new ChannelAccount { Id = result.To },
                ChannelData = result,
                Entities = new List<Entity>(),
                Attachments = new List<Attachment>()
            };
        }
    }
}