using System;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Core.Models
{
    public class InfobipOmniResponse
    {
        [JsonProperty("messages")]
        public InfobipResponseMessage[] Messages { get; set; }
    }

    public class InfobipResponseMessage
    {
        [JsonProperty("to")]
        public InfobipResponseTo To { get; set; }

        [JsonProperty("status")]
        public InfobipResponseStatus Status { get; set; }

        [JsonProperty("messageId")]
        public Guid MessageId { get; set; }
    }

    public class InfobipResponseStatus
    {
        [JsonProperty("groupId")]
        public long GroupId { get; set; }

        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class InfobipResponseTo
    {
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
    }
}