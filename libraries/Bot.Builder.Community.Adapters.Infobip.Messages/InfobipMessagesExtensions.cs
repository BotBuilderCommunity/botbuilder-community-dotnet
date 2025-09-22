using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Infobip.Messages
{
    public static class InfobipMessagesExtensions
    {
        /// <summary>
        /// Add callback data to activity which will be returned to bot in delivery report for that message
        /// </summary>
        /// <param name="activity">Activity to which to add callback data</param>
        /// <param name="callbackData">Callback data which will be returned to bot in delivery report for that message</param>
        public static void AddInfobipCallbackData(this Activity activity, Dictionary<string, string> callbackData)
        {
            if (callbackData == null) throw new ArgumentNullException(nameof(callbackData));

            activity.Entities = activity.Entities ?? new List<Entity>();

            var serializer = new JsonSerializer();
            var entity = new Entity
            {
                Type = InfobipEntityTypes.CallbackData,
                Properties = JObject.FromObject(callbackData, serializer)
            };

            activity.Entities.Add(entity);
        }

        /// <summary>
        /// Get callback data from activity
        /// </summary>
        /// <param name="activity">Activity from which to get callback data</param>
        /// <returns>Callback data</returns>
        public static Dictionary<string, string> GetInfobipCallbackData(this Activity activity)
        {
            if (activity?.Entities == null) return null;

            var entity = activity.Entities.FirstOrDefault(x => x.Type == InfobipEntityTypes.CallbackData);
            return entity?.GetAs<Dictionary<string, string>>();
        }

        /// <summary>
        /// Add Messages API specific content to activity
        /// </summary>
        /// <param name="activity">Activity to which to add content</param>
        /// <param name="content">Messages API content</param>
        public static void AddInfobipMessagesContent(this Activity activity, object content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            activity.Entities = activity.Entities ?? new List<Entity>();

            var serializer = new JsonSerializer();
            var entity = new Entity
            {
                Type = InfobipEntityTypes.MessagesContent,
                Properties = JObject.FromObject(content, serializer)
            };

            activity.Entities.Add(entity);
        }

        /// <summary>
        /// Get Messages API specific content from activity
        /// </summary>
        /// <param name="activity">Activity from which to get content</param>
        /// <returns>Messages API content</returns>
        public static T GetInfobipMessagesContent<T>(this Activity activity) where T : class
        {
            if (activity?.Entities == null) return null;

            var entity = activity.Entities.FirstOrDefault(x => x.Type == InfobipEntityTypes.MessagesContent);
            return entity?.GetAs<T>();
        }

        /// <summary>
        /// Add advanced Messages API options to activity
        /// </summary>
        /// <param name="activity">Activity to which to add options</param>
        /// <param name="options">Advanced message options</param>
        public static void AddInfobipMessagesOptions(this Activity activity, InfobipMessagesCustomOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            activity.AddInfobipMessagesContent(options);
        }

        /// <summary>
        /// Add entity ID to activity for message tracking
        /// </summary>
        /// <param name="activity">Activity to which to add entity ID</param>
        /// <param name="entityId">Entity ID for message tracking</param>
        public static void AddInfobipEntityId(this Activity activity, string entityId)
        {
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException(nameof(entityId));

            var options = activity.GetInfobipMessagesContent<InfobipMessagesCustomOptions>() ?? new InfobipMessagesCustomOptions();
            options.EntityId = entityId;
            activity.AddInfobipMessagesOptions(options);
        }

        /// <summary>
        /// Add application ID to activity for message tracking
        /// </summary>
        /// <param name="activity">Activity to which to add application ID</param>
        /// <param name="applicationId">Application ID for message tracking</param>
        public static void AddInfobipApplicationId(this Activity activity, string applicationId)
        {
            if (string.IsNullOrEmpty(applicationId)) throw new ArgumentNullException(nameof(applicationId));

            var options = activity.GetInfobipMessagesContent<InfobipMessagesCustomOptions>() ?? new InfobipMessagesCustomOptions();
            options.ApplicationId = applicationId;
            activity.AddInfobipMessagesOptions(options);
        }

        /// <summary>
        /// Add validity period to activity
        /// </summary>
        /// <param name="activity">Activity to which to add validity period</param>
        /// <param name="validityPeriod">Validity period value</param>
        /// <param name="timeUnit">Time unit for validity period</param>
        public static void AddInfobipValidityPeriod(this Activity activity, long validityPeriod, string timeUnit = InfobipValidityPeriodTimeUnit.Hours)
        {
            var options = activity.GetInfobipMessagesContent<InfobipMessagesCustomOptions>() ?? new InfobipMessagesCustomOptions();
            options.ValidityPeriod = validityPeriod;
            options.ValidityPeriodTimeUnit = timeUnit;
            activity.AddInfobipMessagesOptions(options);
        }

        /// <summary>
        /// Add URL options to activity for link shortening and tracking
        /// </summary>
        /// <param name="activity">Activity to which to add URL options</param>
        /// <param name="urlOptions">URL options for shortening and tracking</param>
        public static void AddInfobipUrlOptions(this Activity activity, InfobipUrlOptions urlOptions)
        {
            if (urlOptions == null) throw new ArgumentNullException(nameof(urlOptions));

            var options = activity.GetInfobipMessagesContent<InfobipMessagesCustomOptions>() ?? new InfobipMessagesCustomOptions();
            options.UrlOptions = urlOptions;
            activity.AddInfobipMessagesOptions(options);
        }

        /// <summary>
        /// Add regional options to activity for compliance
        /// </summary>
        /// <param name="activity">Activity to which to add regional options</param>
        /// <param name="regionalOptions">Regional options for compliance</param>
        public static void AddInfobipRegionalOptions(this Activity activity, InfobipRegionalOptions regionalOptions)
        {
            if (regionalOptions == null) throw new ArgumentNullException(nameof(regionalOptions));

            var options = activity.GetInfobipMessagesContent<InfobipMessagesCustomOptions>() ?? new InfobipMessagesCustomOptions();
            options.Regional = regionalOptions;
            activity.AddInfobipMessagesOptions(options);
        }

        /// <summary>
        /// Schedule message to be sent at specific time
        /// </summary>
        /// <param name="activity">Activity to schedule</param>
        /// <param name="sendAt">ISO 8601 formatted datetime when to send the message</param>
        public static void AddInfobipSendAt(this Activity activity, string sendAt)
        {
            if (string.IsNullOrEmpty(sendAt)) throw new ArgumentNullException(nameof(sendAt));

            var options = activity.GetInfobipMessagesContent<InfobipMessagesCustomOptions>() ?? new InfobipMessagesCustomOptions();
            options.SendAt = sendAt;
            activity.AddInfobipMessagesOptions(options);
        }

        /// <summary>
        /// Schedule message to be sent at specific time
        /// </summary>
        /// <param name="activity">Activity to schedule</param>
        /// <param name="sendAt">DateTime when to send the message</param>
        public static void AddInfobipSendAt(this Activity activity, DateTime sendAt)
        {
            var iso8601DateTime = sendAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            activity.AddInfobipSendAt(iso8601DateTime);
        }

        // NEW EXTENSION METHODS FOR ENHANCED FEATURES

        /// <summary>
        /// Add WhatsApp template data to activity
        /// </summary>
        /// <param name="activity">Activity to which to add template</param>
        /// <param name="templateName">Name of the WhatsApp template</param>
        /// <param name="templateData">Template data with placeholders</param>
        public static void AddInfobipWhatsAppTemplate(this Activity activity, string templateName, object templateData)
        {
            if (string.IsNullOrEmpty(templateName)) throw new ArgumentNullException(nameof(templateName));
            if (templateData == null) throw new ArgumentNullException(nameof(templateData));

            // Store template info in channel data for converter to use
            var channelData = new Dictionary<string, object>
            {
                ["templateName"] = templateName,
                ["templateData"] = templateData
            };

            activity.ChannelData = channelData;
        }

        /// <summary>
        /// Add interactive list to activity
        /// </summary>
        /// <param name="activity">Activity to convert to interactive list</param>
        /// <param name="title">List title</param>
        /// <param name="sections">List sections with options</param>
        /// <param name="buttonText">Text for the action button (default: "Choose")</param>
        public static void AddInfobipInteractiveList(this Activity activity, string title, InfobipSection[] sections, string buttonText = "Choose")
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));
            if (sections == null || sections.Length == 0) throw new ArgumentNullException(nameof(sections));

            // Store list info in channel data for converter to use
            var channelData = new Dictionary<string, object>
            {
                ["listTitle"] = title,
                ["listSections"] = sections,
                ["listButtonText"] = buttonText
            };

            activity.ChannelData = channelData;
        }

        /// <summary>
        /// Add carousel to activity
        /// </summary>
        /// <param name="activity">Activity to convert to carousel</param>
        /// <param name="cards">Carousel cards</param>
        public static void AddInfobipCarousel(this Activity activity, InfobipCarouselCard[] cards)
        {
            if (cards == null || cards.Length == 0) throw new ArgumentNullException(nameof(cards));

            // Store carousel info in channel data for converter to use
            var channelData = new Dictionary<string, object>
            {
                ["carouselCards"] = cards
            };

            activity.ChannelData = channelData;
        }

        /// <summary>
        /// Add WhatsApp Flow to activity
        /// </summary>
        /// <param name="activity">Activity to add flow to</param>
        /// <param name="flowId">Flow ID</param>
        /// <param name="ctaText">Call to action text</param>
        /// <param name="flowData">Optional flow data</param>
        /// <param name="action">Flow action (navigate or data_exchange)</param>
        /// <param name="navigateScreen">Screen to navigate to (if action is navigate)</param>
        public static void AddInfobipWhatsAppFlow(this Activity activity, string flowId, string ctaText, object flowData = null, string action = InfobipFlowActions.Navigate, string navigateScreen = null)
        {
            if (string.IsNullOrEmpty(flowId)) throw new ArgumentNullException(nameof(flowId));
            if (string.IsNullOrEmpty(ctaText)) throw new ArgumentNullException(nameof(ctaText));

            var content = new InfobipFlowContent
            {
                FlowId = flowId,
                Cta = ctaText,
                Action = action,
                NavigateScreen = navigateScreen,
                FlowData = flowData
            };

            activity.AddInfobipMessagesContent(content);
        }

        /// <summary>
        /// Add location request to activity
        /// </summary>
        /// <param name="activity">Activity to convert to location request</param>
        /// <param name="text">Request text</param>
        public static void AddInfobipLocationRequest(this Activity activity, string text = "Please share your location")
        {
            var content = new InfobipLocationRequestContent
            {
                Text = text
            };

            activity.AddInfobipMessagesContent(content);
        }

        /// <summary>
        /// Add calendar event action to activity
        /// </summary>
        /// <param name="activity">Activity to add calendar event to</param>
        /// <param name="title">Event title</param>
        /// <param name="description">Event description</param>
        /// <param name="startTime">Event start time</param>
        /// <param name="endTime">Event end time</param>
        /// <param name="location">Event location</param>
        public static void AddInfobipCalendarEvent(this Activity activity, string title, string description, DateTime startTime, DateTime endTime, string location = null)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));

            var content = new InfobipCalendarEventContent
            {
                Title = title,
                Description = description,
                StartTime = startTime,
                EndTime = endTime,
                Location = location
            };

            activity.AddInfobipMessagesContent(content);
        }

        /// <summary>
        /// Add reply context to activity (for threaded messages)
        /// </summary>
        /// <param name="activity">Activity to add reply context to</param>
        /// <param name="replyToMessageId">ID of the message being replied to</param>
        public static void AddInfobipReplyContext(this Activity activity, string replyToMessageId)
        {
            if (string.IsNullOrEmpty(replyToMessageId)) throw new ArgumentNullException(nameof(replyToMessageId));

            var content = new InfobipReplyContextContent
            {
                ReplyToMessageId = replyToMessageId
            };

            activity.AddInfobipMessagesContent(content);
        }

        /// <summary>
        /// Add contact sharing to activity
        /// </summary>
        /// <param name="activity">Activity to add contact to</param>
        /// <param name="contact">Contact information</param>
        public static void AddInfobipContact(this Activity activity, InfobipContactInfo contact)
        {
            if (contact == null) throw new ArgumentNullException(nameof(contact));

            var content = new InfobipContactContent
            {
                Contact = contact
            };

            activity.AddInfobipMessagesContent(content);
        }

        /// <summary>
        /// Add sticker to activity
        /// </summary>
        /// <param name="activity">Activity to add sticker to</param>
        /// <param name="stickerUrl">URL of the sticker</param>
        public static void AddInfobipSticker(this Activity activity, string stickerUrl)
        {
            if (string.IsNullOrEmpty(stickerUrl)) throw new ArgumentNullException(nameof(stickerUrl));

            var content = new InfobipStickerContent
            {
                StickerUrl = stickerUrl
            };

            activity.AddInfobipMessagesContent(content);
        }

        // CHANNEL SPECIFICATION METHODS

        /// <summary>
        /// Specify preferred channel for message delivery
        /// </summary>
        /// <param name="activity">Activity to add channel preference to</param>
        /// <param name="preferredChannel">Preferred channel (e.g., "whatsapp", "sms", "viber")</param>
        /// <param name="fallbackChannels">Optional fallback channels if preferred channel fails</param>
        public static void AddInfobipChannelPreference(this Activity activity, string preferredChannel, params string[] fallbackChannels)
        {
            if (string.IsNullOrEmpty(preferredChannel)) throw new ArgumentNullException(nameof(preferredChannel));

            var specification = new InfobipChannelSpecification
            {
                PreferredChannel = preferredChannel,
                FallbackChannels = fallbackChannels?.Length > 0 ? fallbackChannels : null
            };

            activity.Entities = activity.Entities ?? new List<Entity>();

            var serializer = new JsonSerializer();
            var entity = new Entity
            {
                Type = InfobipEntityTypes.ChannelSpecification,
                Properties = JObject.FromObject(specification, serializer)
            };

            activity.Entities.Add(entity);
        }

        /// <summary>
        /// Specify multiple destinations with different channel preferences
        /// </summary>
        /// <param name="activity">Activity to add destination specifications to</param>
        /// <param name="destinations">Array of destination specifications with channel preferences</param>
        public static void AddInfobipDestinations(this Activity activity, InfobipDestinationSpecification[] destinations)
        {
            if (destinations == null || destinations.Length == 0) throw new ArgumentNullException(nameof(destinations));

            var specification = new InfobipChannelSpecification
            {
                Destinations = destinations
            };

            activity.Entities = activity.Entities ?? new List<Entity>();

            var serializer = new JsonSerializer();
            var entity = new Entity
            {
                Type = InfobipEntityTypes.ChannelSpecification,
                Properties = JObject.FromObject(specification, serializer)
            };

            activity.Entities.Add(entity);
        }

        /// <summary>
        /// Add channel options for specific channel configurations
        /// </summary>
        /// <param name="activity">Activity to add channel options to</param>
        /// <param name="channelOptions">Dictionary of channel-specific options</param>
        public static void AddInfobipChannelOptions(this Activity activity, Dictionary<string, object> channelOptions)
        {
            if (channelOptions == null) throw new ArgumentNullException(nameof(channelOptions));

            var existingSpec = activity.GetInfobipMessagesContent<InfobipChannelSpecification>();
            var specification = existingSpec ?? new InfobipChannelSpecification();
            specification.ChannelOptions = channelOptions;

            activity.Entities = activity.Entities ?? new List<Entity>();

            // Remove existing channel specification if present
            if (existingSpec != null)
            {
                var existingEntity = activity.Entities.FirstOrDefault(e => e.Type == InfobipEntityTypes.ChannelSpecification);
                if (existingEntity != null)
                {
                    activity.Entities.Remove(existingEntity);
                }
            }

            var serializer = new JsonSerializer();
            var entity = new Entity
            {
                Type = InfobipEntityTypes.ChannelSpecification,
                Properties = JObject.FromObject(specification, serializer)
            };

            activity.Entities.Add(entity);
        }

        /// <summary>
        /// Specify channel preference using ChannelData (alternative method)
        /// </summary>
        /// <param name="activity">Activity to add channel preference to</param>
        /// <param name="channel">Preferred Infobip channel</param>
        public static void SetInfobipChannelData(this Activity activity, string channel)
        {
            if (string.IsNullOrEmpty(channel)) throw new ArgumentNullException(nameof(channel));

            var channelData = new Dictionary<string, object>
            {
                ["infobipChannel"] = channel
            };

            activity.ChannelData = channelData;
        }

        /// <summary>
        /// Get channel specification from activity
        /// </summary>
        /// <param name="activity">Activity from which to get channel specification</param>
        /// <returns>Channel specification or null if not present</returns>
        public static InfobipChannelSpecification GetInfobipChannelSpecification(this Activity activity)
        {
            if (activity?.Entities == null) return null;

            var entity = activity.Entities.FirstOrDefault(x => x.Type == InfobipEntityTypes.ChannelSpecification);
            return entity?.GetAs<InfobipChannelSpecification>();
        }

        /// <summary>
        /// Set preferred channel for message delivery
        /// </summary>
        /// <param name="activity">Activity to set channel for</param>
        /// <param name="channel">Preferred channel (WHATSAPP, SMS, VIBER_BM, etc.)</param>
        public static void SetInfobipChannel(this Activity activity, string channel)
        {
            if (string.IsNullOrEmpty(channel)) throw new ArgumentNullException(nameof(channel));

            var channelData = new Dictionary<string, object>
            {
                ["infobipChannel"] = channel
            };

            activity.ChannelData = channelData;
        }

        /// <summary>
        /// Set channel using Messages API specific key
        /// </summary>
        /// <param name="activity">Activity to set channel for</param>
        /// <param name="channel">Preferred channel (WHATSAPP, SMS, VIBER_BM, etc.)</param>
        public static void SetInfobipMessagesChannel(this Activity activity, string channel)
        {
            if (string.IsNullOrEmpty(channel)) throw new ArgumentNullException(nameof(channel));

            var channelData = activity.ChannelData as Dictionary<string, object> ?? new Dictionary<string, object>();
            channelData["messagesChannel"] = channel;
            activity.ChannelData = channelData;
        }

        /// <summary>
        /// Set multiple channel options for enhanced control
        /// </summary>
        /// <param name="activity">Activity to set channel options for</param>
        /// <param name="primaryChannel">Primary channel to use</param>
        /// <param name="fallbackChannels">Fallback channels if primary fails</param>
        /// <param name="sender">Optional sender override</param>
        public static void SetInfobipChannelConfiguration(this Activity activity, string primaryChannel, string[] fallbackChannels = null, string sender = null)
        {
            if (string.IsNullOrEmpty(primaryChannel)) throw new ArgumentNullException(nameof(primaryChannel));

            var channelData = activity.ChannelData as Dictionary<string, object> ?? new Dictionary<string, object>();
            channelData["infobipChannel"] = primaryChannel;
            
            if (fallbackChannels?.Length > 0)
            {
                channelData["fallbackChannels"] = fallbackChannels;
            }

            if (!string.IsNullOrEmpty(sender))
            {
                channelData["sender"] = sender;
            }

            activity.ChannelData = channelData;
        }
    }

    public class InfobipMessagesCustomOptions
    {
        public string EntityId { get; set; }
        public string ApplicationId { get; set; }
        public long? ValidityPeriod { get; set; }
        public string ValidityPeriodTimeUnit { get; set; }
        public string SendAt { get; set; }
        public InfobipUrlOptions UrlOptions { get; set; }
        public InfobipRegionalOptions Regional { get; set; }
    }

    // NEW CONTENT CLASSES FOR ENHANCED FEATURES

    public class InfobipTemplateContent
    {
        public string TemplateName { get; set; }
        public InfobipTemplateData TemplateData { get; set; }
    }

    public class InfobipInteractiveListContent
    {
        public string Title { get; set; }
        public InfobipSection[] Sections { get; set; }
        public string ButtonText { get; set; }
    }

    public class InfobipCarouselContent
    {
        public InfobipCarouselCard[] Cards { get; set; }
    }

    public class InfobipFlowContent
    {
        public string FlowId { get; set; }
        public string Cta { get; set; }
        public string Action { get; set; }
        public string NavigateScreen { get; set; }
        public object FlowData { get; set; }
    }

    public class InfobipLocationRequestContent
    {
        public string Text { get; set; }
    }

    public class InfobipCalendarEventContent
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
    }

    public class InfobipReplyContextContent
    {
        public string ReplyToMessageId { get; set; }
    }

    public class InfobipContactContent
    {
        public InfobipContactInfo Contact { get; set; }
    }

    public class InfobipContactInfo
    {
        public string FormattedName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public InfobipContactPhone[] Phones { get; set; }
        public InfobipContactEmail[] Emails { get; set; }
        public string Organization { get; set; }
    }

    public class InfobipContactPhone
    {
        public string Phone { get; set; }
        public string Type { get; set; } // HOME, WORK, MOBILE
    }

    public class InfobipContactEmail
    {
        public string Email { get; set; }
        public string Type { get; set; } // HOME, WORK
    }

    public class InfobipStickerContent
    {
        public string StickerUrl { get; set; }
    }
}