using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Models
{
    /// <summary>
    /// Top-level wrapper for Infobip Messages API requests - matches exact API structure
    /// </summary>
    public class InfobipMessagesRequest
    {
        [JsonProperty("messages")] public InfobipMessage[] Messages { get; set; }
    }

    /// <summary>
    /// Individual message in the Messages API format - matches your JSON samples
    /// </summary>
    public class InfobipMessage
    {
        [JsonProperty("channel")] public string Channel { get; set; }
        [JsonProperty("sender")] public string Sender { get; set; }
        [JsonProperty("destinations")] public InfobipDestination[] Destinations { get; set; }
        [JsonProperty("content")] public InfobipContent Content { get; set; }
        [JsonProperty("template")] public InfobipTemplate Template { get; set; }
        [JsonProperty("messageId")] public string MessageId { get; set; }
        [JsonProperty("callbackData")] public string CallbackData { get; set; }
        [JsonProperty("entityId")] public string EntityId { get; set; }
        [JsonProperty("applicationId")] public string ApplicationId { get; set; }
        [JsonProperty("options")] public InfobipMessageOptions Options { get; set; }
        [JsonProperty("failover")] public InfobipFailover[] Failover { get; set; }
        [JsonProperty("webhooks")] public InfobipWebhooks Webhooks { get; set; }
    }

    public class InfobipDestination
    {
        [JsonProperty("to")] public string To { get; set; }
        [JsonProperty("messageId")] public string MessageId { get; set; }
        [JsonProperty("byChannel")] public InfobipChannelDestination[] ByChannel { get; set; }
    }

    public class InfobipChannelDestination
    {
        [JsonProperty("channel")] public string Channel { get; set; }
        [JsonProperty("to")] public string To { get; set; }
    }

    public class InfobipContent
    {
        [JsonProperty("header")] public InfobipHeader Header { get; set; }
        [JsonProperty("body")] public InfobipBody Body { get; set; }
        [JsonProperty("buttons")] public InfobipButton[] Buttons { get; set; }
        [JsonProperty("footer")] public InfobipFooter Footer { get; set; }
    }

    public class InfobipHeader
    {
        [JsonProperty("type")] public string Type { get; set; } // "TEXT", "IMAGE"
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("1")] public string Placeholder1 { get; set; } // For templates
        [JsonProperty("2")] public string Placeholder2 { get; set; }
    }

    public class InfobipBody
    {
        [JsonProperty("type")] public string Type { get; set; } // "TEXT", "DOCUMENT", "LOCATION", "LIST", "CONTACT", "PRODUCT", "CAROUSEL"
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        
        // Location properties
        [JsonProperty("latitude")] public double? Latitude { get; set; }
        [JsonProperty("longitude")] public double? Longitude { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("address")] public string Address { get; set; }
        
        // List properties  
        [JsonProperty("subtext")] public string Subtext { get; set; }
        [JsonProperty("imageUrl")] public string ImageUrl { get; set; }
        [JsonProperty("sections")] public InfobipSection[] Sections { get; set; }
        
        // Contact properties
        [JsonProperty("phoneNumber")] public string PhoneNumber { get; set; }
        
        // Product properties
        [JsonProperty("catalogId")] public string CatalogId { get; set; }
        [JsonProperty("productRetailerIds")] public string[] ProductRetailerIds { get; set; }
        
        // Carousel properties
        [JsonProperty("cards")] public InfobipCarouselCard[] Cards { get; set; }
        
        // Template placeholders (numbered 1-5 for template parameters)
        [JsonProperty("1")] public string Placeholder1 { get; set; }
        [JsonProperty("2")] public string Placeholder2 { get; set; }
        [JsonProperty("3")] public string Placeholder3 { get; set; }
        [JsonProperty("4")] public string Placeholder4 { get; set; }
        [JsonProperty("5")] public string Placeholder5 { get; set; }
    }

    public class InfobipButton
    {
        [JsonProperty("type")] public string Type { get; set; } // "REPLY", "OPEN_URL", "REQUEST_LOCATION", "QUICK_REPLY"
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("postbackData")] public string PostbackData { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("suffix")] public string Suffix { get; set; } // For template URL buttons
    }

    public class InfobipFooter
    {
        [JsonProperty("text")] public string Text { get; set; }
    }

    public class InfobipSection
    {
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("sectionTitle")] public string SectionTitle { get; set; }
        [JsonProperty("items")] public InfobipListItem[] Items { get; set; }
        [JsonProperty("productRetailerIds")] public string[] ProductRetailerIds { get; set; } // For product sections
    }

    public class InfobipListItem
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("imageUrl")] public string ImageUrl { get; set; }
    }

    // Carousel support
    public class InfobipCarouselCard
    {
        [JsonProperty("header")] public InfobipHeader Header { get; set; }
        [JsonProperty("body")] public InfobipCardBody Body { get; set; }
        [JsonProperty("buttons")] public InfobipButton[] Buttons { get; set; }
    }

    public class InfobipCardBody
    {
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("isVideo")] public bool? IsVideo { get; set; }
        [JsonProperty("cardOptions")] public InfobipCardOptions CardOptions { get; set; }
        [JsonProperty("1")] public string Placeholder1 { get; set; } // For template carousels
        [JsonProperty("2")] public string Placeholder2 { get; set; }
        [JsonProperty("3")] public string Placeholder3 { get; set; }
    }

    public class InfobipCardOptions
    {
        [JsonProperty("orientation")] public string Orientation { get; set; } // "HORIZONTAL", "VERTICAL"
        [JsonProperty("alignment")] public string Alignment { get; set; } // "LEFT", "CENTER", "RIGHT"
        [JsonProperty("height")] public string Height { get; set; } // "SHORT", "MEDIUM", "TALL"
    }

    // Template support
    public class InfobipTemplate
    {
        [JsonProperty("templateName")] public string TemplateName { get; set; }
        [JsonProperty("language")] public string Language { get; set; }
    }

    // Message options
    public class InfobipMessageOptions
    {
        [JsonProperty("validityPeriod")] public InfobipValidityPeriod ValidityPeriod { get; set; }
        [JsonProperty("adaptationMode")] public bool? AdaptationMode { get; set; }
        [JsonProperty("platform")] public InfobipPlatform Platform { get; set; }
    }

    public class InfobipValidityPeriod
    {
        [JsonProperty("amount")] public int Amount { get; set; }
        [JsonProperty("timeUnit")] public string TimeUnit { get; set; } // "SECONDS", "MINUTES", "HOURS"
    }

    public class InfobipPlatform
    {
        [JsonProperty("entityId")] public string EntityId { get; set; }
        [JsonProperty("applicationId")] public string ApplicationId { get; set; }
    }

    // Failover support
    public class InfobipFailover
    {
        [JsonProperty("channel")] public string Channel { get; set; }
        [JsonProperty("sender")] public string Sender { get; set; }
        [JsonProperty("content")] public InfobipContent Content { get; set; }
        [JsonProperty("template")] public InfobipTemplate Template { get; set; }
        [JsonProperty("validityPeriod")] public InfobipValidityPeriod ValidityPeriod { get; set; }
    }

    // Webhooks
    public class InfobipWebhooks
    {
        [JsonProperty("delivery")] public InfobipDeliveryReport Delivery { get; set; }
        [JsonProperty("seen")] public InfobipSeenReport Seen { get; set; }
    }

    public class InfobipDeliveryReport
    {
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("intermediateReport")] public bool? IntermediateReport { get; set; }
        [JsonProperty("receiveTriggeredFailoverReports")] public bool? ReceiveTriggeredFailoverReports { get; set; }
        [JsonProperty("contentType")] public string ContentType { get; set; }
    }

    public class InfobipSeenReport
    {
        [JsonProperty("url")] public string Url { get; set; }
    }

    // Regional Options
    public class InfobipRegionalOptions
    {
        [JsonProperty("indiaDlt")] public InfobipIndiaDltOptions IndiaDlt { get; set; }
        [JsonProperty("turkeyIys")] public InfobipTurkeyIysOptions TurkeyIys { get; set; }
        [JsonProperty("southKorea")] public InfobipSouthKoreaOptions SouthKorea { get; set; }
    }

    public class InfobipIndiaDltOptions
    {
        [JsonProperty("contentTemplateId")] public string ContentTemplateId { get; set; }
        [JsonProperty("principalEntityId")] public string PrincipalEntityId { get; set; }
        [JsonProperty("telemarketerId")] public string TelemarketerId { get; set; }
    }

    public class InfobipTurkeyIysOptions
    {
        [JsonProperty("brandCode")] public int? BrandCode { get; set; }
        [JsonProperty("recipientType")] public string RecipientType { get; set; } // "BIREYSEL", "TACIR"
    }

    public class InfobipSouthKoreaOptions
    {
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("resellerCode")] public int? ResellerCode { get; set; }
    }

    // URL Options
    public class InfobipUrlOptions
    {
        [JsonProperty("shortenUrl")] public bool ShortenUrl { get; set; }
        [JsonProperty("trackClicks")] public bool TrackClicks { get; set; }
        [JsonProperty("trackingUrl")] public string TrackingUrl { get; set; }
        [JsonProperty("removeProtocol")] public bool RemoveProtocol { get; set; }
        [JsonProperty("customDomain")] public string CustomDomain { get; set; }
    }

    // Response models
    public class InfobipMessagesResponse
    {
        [JsonProperty("to")] public string To { get; set; }
        [JsonProperty("messageCount")] public int MessageCount { get; set; }
        [JsonProperty("messageId")] public string MessageId { get; set; }
        [JsonProperty("status")] public InfobipMessagesStatus Status { get; set; }
    }

    public class InfobipMessagesStatus
    {
        [JsonProperty("groupId")] public int GroupId { get; set; }
        [JsonProperty("groupName")] public string GroupName { get; set; }
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
    }

    // Constants based on your JSON samples
    public static class InfobipMessageTypes
    {
        public const string Text = "TEXT";
        public const string Document = "DOCUMENT";
        public const string Location = "LOCATION";
        public const string List = "LIST";
        public const string Contact = "CONTACT";
        public const string Product = "PRODUCT";
        public const string Carousel = "CAROUSEL";
    }

    public static class InfobipButtonTypes
    {
        public const string Reply = "REPLY";
        public const string QuickReply = "QUICK_REPLY";
        public const string OpenUrl = "OPEN_URL";
        public const string RequestLocation = "REQUEST_LOCATION";
        public const string PhoneNumber = "PHONE_NUMBER";
    }

    public static class InfobipChannels
    {
        public const string SMS = "SMS";
        public const string MMS = "MMS";
        public const string WhatsApp = "WHATSAPP";
        public const string ViberBM = "VIBER_BM";
        public const string ViberBot = "VIBER_BOT";
        public const string RCS = "RCS";
        public const string AppleMB = "APPLE_MB";
        public const string InstagramDM = "INSTAGRAM_DM";
        public const string LineON = "LINE_ON";
        public const string Messenger = "MESSENGER";
        public const string GoogleBM = "GOOGLE_BM";
        public const string Telegram = "TELEGRAM";
        public const string Email = "EMAIL";
        public const string Voice = "VOICE";
        public const string Push = "PUSH";
    }

    public static class InfobipValidityPeriodTimeUnit
    {
        public const string Seconds = "SECONDS";
        public const string Minutes = "MINUTES";
        public const string Hours = "HOURS";
    }

    // Template data classes for extension methods
    public class InfobipTemplateData
    {
        [JsonProperty("body")] public InfobipTemplateBodyData Body { get; set; }
        [JsonProperty("header")] public InfobipTemplateHeaderData Header { get; set; }
        [JsonProperty("buttons")] public InfobipTemplateButtonData[] Buttons { get; set; }
    }

    public class InfobipTemplateBodyData
    {
        [JsonProperty("placeholders")] public string[] Placeholders { get; set; }
    }

    public class InfobipTemplateHeaderData
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("placeholders")] public string[] Placeholders { get; set; }
        [JsonProperty("mediaUrl")] public string MediaUrl { get; set; }
        [JsonProperty("filename")] public string Filename { get; set; }
    }

    public class InfobipTemplateButtonData
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("parameter")] public string Parameter { get; set; }
    }

    // Channel specification classes for extension methods
    public class InfobipChannelSpecification
    {
        public string PreferredChannel { get; set; }
        public string[] FallbackChannels { get; set; }
        public InfobipDestinationSpecification[] Destinations { get; set; }
        public Dictionary<string, object> ChannelOptions { get; set; }
    }

    public class InfobipDestinationSpecification
    {
        public string To { get; set; }
        public string Channel { get; set; }
        public string From { get; set; }
        public Dictionary<string, object> Options { get; set; }
    }

    // Backward compatibility
    [System.Obsolete("Use InfobipMessage instead")]
    public class InfobipMessagesOutgoingMessage : InfobipMessage { }

    [System.Obsolete("Use InfobipContent instead")]
    public class InfobipMessagesContent : InfobipContent { }

    [System.Obsolete("Use InfobipBody instead")]
    public class InfobipMessagesBody : InfobipBody { }

    [System.Obsolete("Use InfobipButton instead")]
    public class InfobipMessagesOutgoingButton : InfobipButton { }
}