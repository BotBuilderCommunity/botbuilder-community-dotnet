namespace Bot.Builder.Community.Adapters.Infobip.Messages
{
    public static class InfobipMessagesConstants
    {
        public const string ChannelName = "infobip-messages";
        public const string ChannelId = "infobip-messages";
    }

    public static class InfobipMessagesMessageTypes
    {
        public const string Text = "text";
        public const string Image = "image";
        public const string Document = "document";
        public const string Video = "video";
        public const string Audio = "audio";
        public const string Sticker = "sticker";
        public const string Location = "location";
        public const string Contact = "contact";
        public const string Interactive = "interactive";
        public const string Button = "button";
        public const string List = "list";
        public const string Template = "template";
        public const string Flow = "flow";
        public const string Carousel = "carousel";
    }

    public static class InfobipInteractiveTypes
    {
        public const string Button = "button";
        public const string List = "list";
        public const string LocationRequestMessage = "location_request_message";
        public const string CatalogMessage = "catalog_message";
        public const string ProductMessage = "product_message";
        public const string ProductListMessage = "product_list_message";
        public const string Flow = "flow";
    }

    public static class InfobipButtonTypes
    {
        public const string QuickReply = "QUICK_REPLY";
        public const string Url = "URL";
        public const string PhoneNumber = "PHONE_NUMBER";
        public const string CopyCode = "COPY_CODE";
        public const string Calendar = "CALENDAR";
        public const string FlowAction = "FLOW_ACTION";
    }

    public static class InfobipHeaderTypes
    {
        public const string Text = "TEXT";
        public const string Image = "IMAGE";
        public const string Video = "VIDEO";
        public const string Document = "DOCUMENT";
    }

    public static class InfobipFlowActions
    {
        public const string Navigate = "navigate";
        public const string DataExchange = "data_exchange";
    }

    public static class InfobipContactTypes
    {
        public const string Home = "HOME";
        public const string Work = "WORK";
        public const string Mobile = "MOBILE";
    }

    public static class InfobipValidityPeriodTimeUnit
    {
        public const string Nanoseconds = "NANOSECONDS";
        public const string Microseconds = "MICROSECONDS";
        public const string Milliseconds = "MILLISECONDS";
        public const string Seconds = "SECONDS";
        public const string Minutes = "MINUTES";
        public const string Hours = "HOURS";
        public const string Days = "DAYS";
    }

    public static class InfobipAdaptationMode
    {
        public const string Strict = "STRICT";
        public const string Relaxed = "RELAXED";
        public const string Flexible = "FLEXIBLE";
    }

    public static class InfobipTemplateButtonTypes
    {
        public const string QuickReply = "QUICK_REPLY";
        public const string Url = "URL";
        public const string PhoneNumber = "PHONE_NUMBER";
        public const string FlowAction = "FLOW";
        public const string CopyCode = "COPY_CODE";
        public const string Calendar = "CALENDAR";
    }

    public static class InfobipLanguageCodes
    {
        public const string English = "en";
        public const string Spanish = "es";
        public const string French = "fr";
        public const string German = "de";
        public const string Italian = "it";
        public const string Portuguese = "pt";
        public const string Russian = "ru";
        public const string Chinese = "zh";
        public const string Japanese = "ja";
        public const string Korean = "ko";
        public const string Arabic = "ar";
        public const string Hindi = "hi";
        public const string Indonesian = "id";
        public const string Malay = "ms";
        public const string Thai = "th";
        public const string Vietnamese = "vi";
        public const string Turkish = "tr";
        public const string Dutch = "nl";
        public const string Polish = "pl";
        public const string Czech = "cs";
        public const string Hungarian = "hu";
        public const string Romanian = "ro";
        public const string Bulgarian = "bg";
        public const string Croatian = "hr";
        public const string Slovak = "sk";
        public const string Slovenian = "sl";
        public const string Estonian = "et";
        public const string Latvian = "lv";
        public const string Lithuanian = "lt";
        public const string Finnish = "fi";
        public const string Swedish = "sv";
        public const string Norwegian = "no";
        public const string Danish = "da";
        public const string Icelandic = "is";
    }

    public static class InfobipChannelTypes
    {
        public const string WhatsApp = "whatsapp";
        public const string SMS = "sms";
        public const string RCS = "rcs";
        public const string Viber = "viber";
        public const string Facebook = "facebook";
        public const string Instagram = "instagram";
        public const string Line = "line";
        public const string Telegram = "telegram";
        public const string GoogleBM = "gbm";
        public const string Apple = "applebc";
        public const string WeChat = "wechat";
        public const string KakaoTalk = "kakaotalk";
        public const string Email = "email";
        public const string Voice = "voice";
        public const string MMS = "mms";
    }

    public static class InfobipFileTypes
    {
        // Image types
        public const string ImageJpeg = "image/jpeg";
        public const string ImagePng = "image/png";
        public const string ImageGif = "image/gif";
        public const string ImageWebp = "image/webp";

        // Video types
        public const string VideoMp4 = "video/mp4";
        public const string Video3gp = "video/3gp";

        // Audio types
        public const string AudioAac = "audio/aac";
        public const string AudioMp4 = "audio/mp4";
        public const string AudioMpeg = "audio/mpeg";
        public const string AudioAmr = "audio/amr";
        public const string AudioOgg = "audio/ogg";

        // Document types
        public const string ApplicationPdf = "application/pdf";
        public const string ApplicationDoc = "application/msword";
        public const string ApplicationDocx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string ApplicationXls = "application/vnd.ms-excel";
        public const string ApplicationXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string ApplicationPpt = "application/vnd.ms-powerpoint";
        public const string ApplicationPptx = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
    }

    public static class InfobipEntityTypes
    {
        public const string CallbackData = "infobip.callback.data";
        public const string MessagesContent = "infobip.messages.content";
        public const string TemplateContent = "infobip.template.content";
        public const string InteractiveContent = "infobip.interactive.content";
        public const string CarouselContent = "infobip.carousel.content";
        public const string FlowContent = "infobip.flow.content";
        public const string LocationContent = "infobip.location.content";
        public const string ContactContent = "infobip.contact.content";
        public const string StickerContent = "infobip.sticker.content";
        public const string CalendarContent = "infobip.calendar.content";
        public const string ReplyContext = "infobip.reply.context";
        public const string ChannelSpecification = "infobip.channel.specification";
    }

    public static class InfobipEventTypes
    {
        public const string Delivery = "DELIVERY";
        public const string Seen = "SEEN";
        public const string Sent = "SENT";
        public const string Failed = "FAILED";
        public const string Pending = "PENDING";
        public const string Expired = "EXPIRED";
        public const string Rejected = "REJECTED";
        public const string Unknown = "UNKNOWN";
    }

    public static class InfobipErrorCodes
    {
        public const string InvalidApiKey = "INVALID_API_KEY";
        public const string InvalidRequestFormat = "INVALID_REQUEST_FORMAT";
        public const string UnsupportedMessageType = "UNSUPPORTED_MESSAGE_TYPE";
        public const string MediaTooLarge = "MEDIA_TOO_LARGE";
        public const string InvalidMediaType = "INVALID_MEDIA_TYPE";
        public const string TemplateNotFound = "TEMPLATE_NOT_FOUND";
        public const string InvalidTemplateData = "INVALID_TEMPLATE_DATA";
        public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
        public const string ChannelNotSupported = "CHANNEL_NOT_SUPPORTED";
        public const string InteractiveNotSupported = "INTERACTIVE_NOT_SUPPORTED";
        public const string FlowNotFound = "FLOW_NOT_FOUND";
        public const string InvalidFlowData = "INVALID_FLOW_DATA";
    }

    public static class InfobipPlatformLimits
    {
        // WhatsApp limits
        public const int WhatsAppMaxTextLength = 4096;
        public const int WhatsAppMaxCaptionLength = 1024;
        public const int WhatsAppMaxButtonsPerMessage = 3;
        public const int WhatsAppMaxCarouselCards = 10;
        public const int WhatsAppMaxListSections = 10;
        public const int WhatsAppMaxListRowsPerSection = 10;
        public const int WhatsAppMaxHeaderLength = 60;
        public const int WhatsAppMaxFooterLength = 60;
        public const int WhatsAppMaxButtonTitleLength = 20;
        public const int WhatsAppMaxListRowTitleLength = 24;
        public const int WhatsAppMaxListRowDescriptionLength = 72;

        // SMS limits
        public const int SmsMaxTextLength = 1600;
        public const int SmsMaxSingleMessageLength = 160;

        // General limits
        public const long MaxMediaSize = 16 * 1024 * 1024; // 16MB
        public const int MaxCallbackDataLength = 4000;
        public const int MaxTemplateParameterLength = 256;
    }
}