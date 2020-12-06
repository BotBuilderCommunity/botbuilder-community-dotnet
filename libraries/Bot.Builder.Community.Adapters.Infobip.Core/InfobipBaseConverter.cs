using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Infobip.Core
{
    public abstract class InfobipBaseConverter
    {
        protected static Activity CreateBaseDeliveryReportActivity(InfobipIncomingResultBase response)
        {
            var activity = ConvertToEvent(response);
            activity.Name = InfobipReportTypes.DELIVERY;
            activity.Timestamp = response.DoneAt;
            return activity;
        }

        protected static Activity ConvertToMessage(InfobipIncomingResultBase result)
        {
            var activity = Convert(result);

            activity.Type = ActivityTypes.Message;
            activity.From = new ChannelAccount { Id = result.From };
            activity.Conversation = new ConversationAccount { IsGroup = false, Id = result.From };
            activity.Timestamp = result.ReceivedAt;

            return activity;
        }

        protected static Activity ConvertToEvent(InfobipIncomingResultBase result)
        {
            var activity = Convert(result);
            activity.Type = ActivityTypes.Event;
            activity.Conversation = new ConversationAccount { Id = result.To };
            activity.Text = null;

            return activity;
        }

        private static Activity Convert(InfobipIncomingResultBase result)
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