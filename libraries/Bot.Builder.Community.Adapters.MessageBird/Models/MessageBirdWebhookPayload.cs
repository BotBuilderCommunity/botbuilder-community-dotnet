using System;
using System.Collections.Generic;
using MessageBird.Objects.Conversations;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;


namespace Bot.Builder.Community.Adapters.MessageBird.Models
{
    public class MessageBirdWebhookPayload
    {
        public Contact contact { get; set; }
        public Conversation conversation { get; set; }
        public Message message { get; set; }
        public string type { get; set; }
    }

    public class Message
    {
        public string id { get; set; }
        public string conversationId { get; set; }
        public string platform { get; set; }
        public string to { get; set; }
        public string from { get; set; }
        public string channelId { get; set; }
        public string type { get; set; }
        public Content content { get; set; }
        public string direction { get; set; }
        public string status { get; set; }
        public DateTime createdDatetime { get; set; }
        public string updatedDatetime { get; set; }
    }
}
