using System;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Adapters.Infobip.Messages
{
    public class InfobipMessagesAdapterOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipMessagesAdapterOptions"/> class using appsettings.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <remarks>
        /// The configuration keys are:
        ///     InfobipApiKey: An Infobip API key.
        ///     InfobipMessagesApiBaseUrl: The Infobip Messages API base url.
        ///     InfobipAppSecret: A secret used to validate that incoming webhooks are originated from Infobip.
        ///     InfobipMessagesApiKey: The API key for Messages API.
        ///     DefaultEntityId: Default entity ID for message tracking.
        ///     DefaultApplicationId: Default application ID for message tracking.
        ///     DefaultValidityPeriod: Default message validity period.
        ///     DefaultValidityPeriodTimeUnit: Default time unit for validity period.
        ///     AdaptationMode: Message adaptation mode (STRICT, RELAXED, FLEXIBLE).
        ///     EnableUrlShortening: Enable automatic URL shortening.
        ///     EnableUrlTracking: Enable automatic URL click tracking.
        ///     EnableInteractiveMessaging: Enable interactive messaging features.
        ///     EnableWhatsAppTemplates: Enable WhatsApp template message support.
        ///     EnableCarouselSupport: Enable carousel message support.
        ///     EnableLocationSharing: Enable location sharing features.
        ///     MaxCarouselCards: Maximum number of cards in carousel (default: 10).
        ///     MaxInteractiveButtons: Maximum number of interactive buttons (default: 3).
        ///     MaxListSections: Maximum number of list sections (default: 10).
        ///     MaxListRows: Maximum number of rows per list section (default: 10).
        /// </remarks>
        public InfobipMessagesAdapterOptions(IConfiguration configuration)
            : this(configuration["InfobipApiKey"],
                configuration["InfobipMessagesApiBaseUrl"],
                configuration["InfobipAppSecret"])
        {
            InfobipApiKey = configuration["InfobipMessagesApiKey"] ?? configuration["InfobipApiKey"];
            
            // Advanced configuration
            DefaultEntityId = configuration["DefaultEntityId"];
            DefaultApplicationId = configuration["DefaultApplicationId"];
            DefaultSender = configuration["DefaultSender"];
            DefaultChannel = configuration["DefaultChannel"] ?? InfobipChannels.WhatsApp;
            
            if (long.TryParse(configuration["DefaultValidityPeriod"], out var validityPeriod))
                DefaultValidityPeriod = validityPeriod;
                
            DefaultValidityPeriodTimeUnit = configuration["DefaultValidityPeriodTimeUnit"] ?? InfobipValidityPeriodTimeUnit.Hours;
            AdaptationMode = configuration["AdaptationMode"] ?? InfobipAdaptationMode.Flexible;
            
            if (bool.TryParse(configuration["EnableUrlShortening"], out var enableUrlShortening))
                EnableUrlShortening = enableUrlShortening;
                
            if (bool.TryParse(configuration["EnableUrlTracking"], out var enableUrlTracking))
                EnableUrlTracking = enableUrlTracking;
                
            if (bool.TryParse(configuration["EnableInteractiveMessaging"], out var enableInteractive))
                EnableInteractiveMessaging = enableInteractive;
                
            if (bool.TryParse(configuration["EnableWhatsAppTemplates"], out var enableTemplates))
                EnableWhatsAppTemplates = enableTemplates;
                
            if (bool.TryParse(configuration["EnableCarouselSupport"], out var enableCarousel))
                EnableCarouselSupport = enableCarousel;
                
            if (bool.TryParse(configuration["EnableLocationSharing"], out var enableLocation))
                EnableLocationSharing = enableLocation;
                
            if (int.TryParse(configuration["MaxCarouselCards"], out var maxCarouselCards))
                MaxCarouselCards = maxCarouselCards;
                
            if (int.TryParse(configuration["MaxInteractiveButtons"], out var maxButtons))
                MaxInteractiveButtons = maxButtons;
                
            if (int.TryParse(configuration["MaxListSections"], out var maxSections))
                MaxListSections = maxSections;
                
            if (int.TryParse(configuration["MaxListRows"], out var maxRows))
                MaxListRows = maxRows;
                
            DefaultNotifyUrl = configuration["DefaultNotifyUrl"];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipMessagesAdapterOptions"/> class.
        /// </summary>
        /// <param name="apiKey">An Infobip API key.</param>
        /// <param name="apiBaseUrl">The Infobip Messages API base url.</param>
        /// <param name="appSecret">A secret used to validate that incoming webhooks are originated from Infobip.</param>
        public InfobipMessagesAdapterOptions(string apiKey, string apiBaseUrl, string appSecret = null)
        {
            InfobipApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            InfobipMessagesApiBaseUrl = apiBaseUrl ?? throw new ArgumentNullException(nameof(apiBaseUrl));
            InfobipAppSecret = appSecret;
            
            // Set defaults for properties that aren't set by the parameterized constructor
            DefaultChannel = InfobipChannels.WhatsApp;
            AdaptationMode = InfobipAdaptationMode.Flexible;
            DefaultValidityPeriodTimeUnit = InfobipValidityPeriodTimeUnit.Hours;
        }

        /// <summary>
        /// Infobip API key for authentication
        /// </summary>
        public string InfobipApiKey { get; set; }

        /// <summary>
        /// Base URL for Infobip Messages API
        /// </summary>
        public string InfobipMessagesApiBaseUrl { get; set; }

        /// <summary>
        /// App secret for webhook verification (optional)
        /// </summary>
        public string InfobipAppSecret { get; set; }

        /// <summary>
        /// Default entity ID for messages (optional)
        /// </summary>
        public string DefaultEntityId { get; set; }

        /// <summary>
        /// Default application ID for messages (optional)
        /// </summary>
        public string DefaultApplicationId { get; set; }

        /// <summary>
        /// Default validity period for messages in specified time units (optional)
        /// </summary>
        public long? DefaultValidityPeriod { get; set; }

        /// <summary>
        /// Default time unit for validity period (optional)
        /// </summary>
        public string DefaultValidityPeriodTimeUnit { get; set; }

        /// <summary>
        /// Default URL options for link shortening and tracking (optional)
        /// </summary>
        public InfobipUrlOptions DefaultUrlOptions { get; set; }

        /// <summary>
        /// Default regional options for compliance (optional)
        /// </summary>
        public InfobipRegionalOptions DefaultRegionalOptions { get; set; }

        /// <summary>
        /// Adaptation mode for handling channel-specific features
        /// </summary>
        public string AdaptationMode { get; set; } = InfobipAdaptationMode.Flexible;

        /// <summary>
        /// Enable automatic URL shortening for outgoing messages
        /// </summary>
        public bool EnableUrlShortening { get; set; } = false;

        /// <summary>
        /// Enable automatic URL click tracking for outgoing messages
        /// </summary>
        public bool EnableUrlTracking { get; set; } = false;

        /// <summary>
        /// Default notification URL for delivery reports (optional)
        /// </summary>
        public string DefaultNotifyUrl { get; set; }

        // NEW ENHANCED FEATURES OPTIONS

        /// <summary>
        /// Enable interactive messaging features (buttons, lists, carousels)
        /// </summary>
        public bool EnableInteractiveMessaging { get; set; } = true;

        /// <summary>
        /// Enable WhatsApp template message support
        /// </summary>
        public bool EnableWhatsAppTemplates { get; set; } = true;

        /// <summary>
        /// Enable carousel message support
        /// </summary>
        public bool EnableCarouselSupport { get; set; } = true;

        /// <summary>
        /// Enable location sharing features
        /// </summary>
        public bool EnableLocationSharing { get; set; } = true;

        /// <summary>
        /// Enable WhatsApp Flow integration
        /// </summary>
        public bool EnableWhatsAppFlows { get; set; } = true;

        /// <summary>
        /// Enable contact sharing features
        /// </summary>
        public bool EnableContactSharing { get; set; } = true;

        /// <summary>
        /// Enable sticker support
        /// </summary>
        public bool EnableStickers { get; set; } = true;

        /// <summary>
        /// Enable calendar event actions
        /// </summary>
        public bool EnableCalendarEvents { get; set; } = true;

        /// <summary>
        /// Enable reply context for threaded messages
        /// </summary>
        public bool EnableReplyContext { get; set; } = true;

        /// <summary>
        /// Automatically convert suggested actions to interactive lists when count > threshold
        /// </summary>
        public bool AutoConvertToInteractiveList { get; set; } = true;

        /// <summary>
        /// Threshold for converting suggested actions to interactive list (default: 3)
        /// </summary>
        public int InteractiveListThreshold { get; set; } = 3;

        /// <summary>
        /// Automatically convert multiple hero cards to carousel
        /// </summary>
        public bool AutoConvertToCarousel { get; set; } = true;

        /// <summary>
        /// Maximum number of cards in carousel (platform limit, default: 10)
        /// </summary>
        public int MaxCarouselCards { get; set; } = 10;

        /// <summary>
        /// Maximum number of interactive buttons per message (platform limit, default: 3)
        /// </summary>
        public int MaxInteractiveButtons { get; set; } = 3;

        /// <summary>
        /// Maximum number of sections in interactive list (platform limit, default: 10)
        /// </summary>
        public int MaxListSections { get; set; } = 10;

        /// <summary>
        /// Maximum number of rows per list section (platform limit, default: 10)
        /// </summary>
        public int MaxListRows { get; set; } = 10;

        /// <summary>
        /// Fallback behavior when interactive features are not supported by channel
        /// </summary>
        public InfobipFallbackBehavior FallbackBehavior { get; set; } = InfobipFallbackBehavior.ConvertToText;

        /// <summary>
        /// Default language code for templates and localized content
        /// </summary>
        public string DefaultLanguageCode { get; set; } = "en";

        /// <summary>
        /// Enable automatic media optimization
        /// </summary>
        public bool EnableMediaOptimization { get; set; } = true;

        /// <summary>
        /// Maximum file size for media attachments in bytes (default: 16MB)
        /// </summary>
        public long MaxMediaSize { get; set; } = 16 * 1024 * 1024; // 16MB

        /// <summary>
        /// Enable delivery report tracking
        /// </summary>
        public bool EnableDeliveryReports { get; set; } = true;

        /// <summary>
        /// Enable seen report tracking (where supported)
        /// </summary>
        public bool EnableSeenReports { get; set; } = true;

        /// <summary>
        /// Timeout for API requests in seconds (default: 30)
        /// </summary>
        public int ApiTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Number of retry attempts for failed API calls (default: 3)
        /// </summary>
        public int RetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts in milliseconds (default: 1000)
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 1000;

        /// <summary>
        /// Default sender ID for messages
        /// </summary>
        public string DefaultSender { get; set; }

        /// <summary>
        /// Enable automatic channel detection from Bot Framework ChannelId
        /// </summary>
        public bool EnableAutomaticChannelDetection { get; set; } = true;

        /// <summary>
        /// Default channel for messages (default: WHATSAPP)
        /// </summary>
        public string DefaultChannel { get; set; } = InfobipChannels.WhatsApp;

        /// <summary>
        /// Enable adaptation mode for channel-specific feature adaptation
        /// </summary>
        public bool EnableAdaptationMode { get; set; } = true;
    }

    /// <summary>
    /// Fallback behavior for unsupported features
    /// </summary>
    public enum InfobipFallbackBehavior
    {
        /// <summary>
        /// Convert interactive elements to plain text
        /// </summary>
        ConvertToText,
        
        /// <summary>
        /// Convert interactive elements to simple buttons where possible
        /// </summary>
        ConvertToButtons,
        
        /// <summary>
        /// Skip unsupported elements
        /// </summary>
        Skip,
        
        /// <summary>
        /// Throw exception for unsupported elements
        /// </summary>
        ThrowException
    }
}