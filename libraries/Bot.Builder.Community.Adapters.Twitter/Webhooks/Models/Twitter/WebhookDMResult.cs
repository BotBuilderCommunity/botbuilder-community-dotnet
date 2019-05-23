using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter
{

    public class WebhookDMResult
    {
        [JsonProperty("direct_message_events")]
        public List<DMEvent> Events { get; set; }

        [JsonProperty("users")]
        public Dictionary<string, TwitterUser> Users { get; set; }

        public static implicit operator DirectMessageEvent(WebhookDMResult result)
        {
            var dmEvent = new DirectMessageEvent();

            var tdmEvent = result.Events.First();

            dmEvent.Id = tdmEvent.id;
            dmEvent.Created = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(tdmEvent.created_timestamp);
            dmEvent.Type = tdmEvent.type == "message_create" ? TwitterEventType.MessageCreate : TwitterEventType.Unknown;

            dmEvent.MessageEntities = tdmEvent.message_create?.message_data?.entities;
            if (tdmEvent.message_create.message_data.attachment != null)
            {
                dmEvent.MessageEntities.media = new List<MediaEntity>();
                dmEvent.MessageEntities.media.Add(tdmEvent.message_create.message_data.attachment.media);
            }
           

            dmEvent.MessageText = tdmEvent.message_create?.message_data?.text;
            dmEvent.Recipient = result.Users.Where(u => u.Value.Id == tdmEvent.message_create.target.recipient_id)?.FirstOrDefault().Value;
            dmEvent.Sender = result.Users.Where(u => u.Value.Id == tdmEvent.message_create.sender_id)?.FirstOrDefault().Value;

            return dmEvent;
        }
    }

    public class NewDmResult
    {
        public DMEvent @event { get; set; }
    }

    public class DMEvent
    {
        public string type { get; set; }
        public string id { get; set; }
        public long created_timestamp { get; set; }
        public Message message_create { get; set; }

        public static implicit operator DirectMessageResult(DMEvent dmevent)
        {
            var res = new DirectMessageResult();
            res.Created = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(dmevent.created_timestamp);
            res.Id = dmevent.id;
            res.MessageText = dmevent.message_create.message_data.text;
            res.RecipientId = dmevent.message_create.target.recipient_id;
            res.SenderId = dmevent.message_create.sender_id;

            return res;
        }
    }

    public class Message
    {
        public Target target { get; set; }
        public string sender_id { get; set; }
        public Message_Data message_data { get; set; }
    }

    public class Target
    {
        public string recipient_id { get; set; }
    }

    public class Message_Data
    {
        public string text { get; set; }
        public TwitterEntities entities { get; set; }
        public Attachment attachment { get; set; }
    }

    public class Attachment
    {
        public string type { get; set; }
        public MediaEntity media { get; set; }

    }

}
