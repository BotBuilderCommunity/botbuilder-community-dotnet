using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Newtonsoft.Json;
using System;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Models
{
    public class InfobipMessagesIncomingResult : InfobipIncomingResultBase
    {
        [JsonProperty("message")] public InfobipMessagesIncomingMessage Message { get; set; }
        [JsonProperty("contact")] public InfobipMessagesContact Contact { get; set; }
        [JsonProperty("platform")] public string Platform { get; set; }

        /// <summary>
        /// when message was seen in ISO8601 date time format
        /// </summary>
        [JsonProperty("seenAt")] public DateTimeOffset? SeenAt { get; set; }

        /// <summary>
        /// Returns True if this message represents seen report
        /// </summary>
        /// <returns>True if this message represents seen report</returns>
        public bool IsSeenReport()
        {
            return SeenAt != null;
        }

        /// <summary>
        /// Returns True if this message represents message sent by subscriber to bot
        /// </summary>
        /// <returns>True if this message represents message sent by subscriber to bot</returns>
        public bool IsMessage()
        {
            return Message != null;
        }
    }

    public class InfobipMessagesContact
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("profile")] public InfobipMessagesContactProfile Profile { get; set; }
    }

    public class InfobipMessagesContactProfile
    {
        [JsonProperty("name")] public string Name { get; set; }
    }

    public class InfobipMessagesIncomingMessage
    {
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("caption")] public string Caption { get; set; }
        [JsonProperty("location")] public InfobipMessagesLocation Location { get; set; }
        [JsonProperty("contact")] public InfobipMessagesContactInfo ContactInfo { get; set; }
        [JsonProperty("interactive")] public InfobipMessagesInteractive Interactive { get; set; }
        [JsonProperty("button")] public InfobipMessagesIncomingButton Button { get; set; }
        [JsonProperty("listReply")] public InfobipMessagesListReply ListReply { get; set; }

        /// <summary>
        /// Returns True if this message represents media message
        /// </summary>
        /// <returns>True if this message represents media message</returns>
        public bool IsMedia()
        {
            var mediaTypes = new[]
            {
                "audio", "document", "image", "video", "sticker"
            };

            return Type != null && Array.Exists(mediaTypes, type => type.Equals(Type, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class InfobipMessagesLocation
    {
        [JsonProperty("latitude")] public double Latitude { get; set; }
        [JsonProperty("longitude")] public double Longitude { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("address")] public string Address { get; set; }
    }

    public class InfobipMessagesContactInfo
    {
        [JsonProperty("name")] public InfobipMessagesContactName Name { get; set; }
        [JsonProperty("phones")] public InfobipMessagesContactPhone[] Phones { get; set; }
        [JsonProperty("emails")] public InfobipMessagesContactEmail[] Emails { get; set; }
    }

    public class InfobipMessagesContactName
    {
        [JsonProperty("formattedName")] public string FormattedName { get; set; }
        [JsonProperty("firstName")] public string FirstName { get; set; }
        [JsonProperty("lastName")] public string LastName { get; set; }
    }

    public class InfobipMessagesContactPhone
    {
        [JsonProperty("phone")] public string Phone { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
    }

    public class InfobipMessagesContactEmail
    {
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
    }

    public class InfobipMessagesInteractive
    {
        [JsonProperty("buttonReply")] public InfobipMessagesButtonReply ButtonReply { get; set; }
        [JsonProperty("listReply")] public InfobipMessagesListReply ListReply { get; set; }
    }

    public class InfobipMessagesButtonReply
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
    }

    public class InfobipMessagesListReply
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
    }

    public class InfobipMessagesIncomingButton
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
    }
}