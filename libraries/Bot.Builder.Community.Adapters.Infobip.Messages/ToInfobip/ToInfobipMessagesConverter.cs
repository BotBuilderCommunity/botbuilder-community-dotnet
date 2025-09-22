using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.ToInfobip
{
    public class ToInfobipMessagesConverter
    {
        private readonly InfobipMessagesAdapterOptions _adapterOptions;
        private readonly ILogger _logger;

        public ToInfobipMessagesConverter(InfobipMessagesAdapterOptions adapterOptions, ILogger logger)
        {
            _adapterOptions = adapterOptions ?? throw new ArgumentNullException(nameof(adapterOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Converts Bot Framework activity to Infobip Messages API request
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <param name="conversationId">The conversation ID</param>
        /// <returns>Infobip Messages API request</returns>
        public Task<InfobipMessagesRequest> Convert(Activity activity, string conversationId)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));

            _logger.LogInformation("?? Converting Bot Framework Activity to Infobip Messages API request");
            _logger.LogInformation($"?? Activity ID: {activity.Id}");
            _logger.LogInformation($"?? Original Conversation ID: {conversationId}");
            _logger.LogInformation($"?? Bot Framework Channel ID: {activity.ChannelId}");

            var channel = GetChannelFromActivity(activity);
            var sender = GetSenderFromActivity(activity);
            var recipient = GetRecipientWithPriority(activity, conversationId);

            _logger.LogInformation($"?? Final Channel: {channel}");
            _logger.LogInformation($"?? Final Sender: {sender}");
            _logger.LogInformation($"?? Final Recipient: {recipient}");

            var message = new InfobipMessage
            {
                Channel = channel,
                Sender = sender,
                Destinations = new[]
                {
                    new InfobipDestination { To = recipient }
                },
                MessageId = activity.Id,
                EntityId = _adapterOptions.DefaultEntityId,
                ApplicationId = _adapterOptions.DefaultApplicationId
            };

            // Log destination and IDs
            _logger.LogInformation($"?? Message Destination: {recipient}");
            _logger.LogInformation($"??? Entity ID: {message.EntityId}");
            _logger.LogInformation($"?? Application ID: {message.ApplicationId}");

            // Handle message options
            var options = new InfobipMessageOptions();
            bool hasOptions = false;

            // Handle validity period
            if (_adapterOptions.DefaultValidityPeriod.HasValue)
            {
                options.ValidityPeriod = new InfobipValidityPeriod
                {
                    Amount = (int)_adapterOptions.DefaultValidityPeriod.Value,
                    TimeUnit = _adapterOptions.DefaultValidityPeriodTimeUnit ?? InfobipValidityPeriodTimeUnit.Hours
                };
                hasOptions = true;
                _logger.LogInformation($"? Validity period: {options.ValidityPeriod.Amount} {options.ValidityPeriod.TimeUnit}");
            }

            // Handle adaptation mode
            if (_adapterOptions.EnableAdaptationMode)
            {
                options.AdaptationMode = true;
                hasOptions = true;
                _logger.LogInformation("?? Adaptation mode enabled");
            }

            // Handle platform options
            if (!string.IsNullOrEmpty(_adapterOptions.DefaultEntityId) || !string.IsNullOrEmpty(_adapterOptions.DefaultApplicationId))
            {
                options.Platform = new InfobipPlatform
                {
                    EntityId = _adapterOptions.DefaultEntityId,
                    ApplicationId = _adapterOptions.DefaultApplicationId
                };
                hasOptions = true;
                _logger.LogInformation($"??? Platform options: Entity={options.Platform.EntityId}, App={options.Platform.ApplicationId}");
            }

            if (hasOptions)
            {
                message.Options = options;
            }

            // Handle callback data
            var callbackData = activity.GetInfobipCallbackData();
            if (callbackData != null && callbackData.Any())
            {
                message.CallbackData = JsonConvert.SerializeObject(callbackData);
                _logger.LogInformation($"?? Callback data: {message.CallbackData}");
            }

            // Handle webhooks configuration
            if (!string.IsNullOrEmpty(_adapterOptions.DefaultNotifyUrl))
            {
                message.Webhooks = new InfobipWebhooks
                {
                    Delivery = new InfobipDeliveryReport
                    {
                        Url = _adapterOptions.DefaultNotifyUrl,
                        IntermediateReport = _adapterOptions.EnableDeliveryReports,
                        ContentType = "application/json"
                    }
                };

                if (_adapterOptions.EnableSeenReports)
                {
                    message.Webhooks.Seen = new InfobipSeenReport
                    {
                        Url = _adapterOptions.DefaultNotifyUrl
                    };
                }

                _logger.LogInformation($"?? Webhook URL: {_adapterOptions.DefaultNotifyUrl}");
                _logger.LogInformation($"?? Delivery reports: {_adapterOptions.EnableDeliveryReports}");
                _logger.LogInformation($"??? Seen reports: {_adapterOptions.EnableSeenReports}");
            }

            // Convert activity content to message content
            message.Content = ConvertActivityToContent(activity);

            var request = new InfobipMessagesRequest
            {
                Messages = new[] { message }
            };

            // Ensure channel field is always present and valid
            if (string.IsNullOrEmpty(message.Channel))
            {
                message.Channel = InfobipChannels.WhatsApp;
                _logger.LogWarning($"?? Channel was null/empty, using default: {message.Channel}");
            }

            // Ensure sender field is always present and valid
            if (string.IsNullOrEmpty(message.Sender))
            {
                _logger.LogError("? CRITICAL: Sender field is missing! This will cause API call to fail.");
                throw new InvalidOperationException("Sender field cannot be null or empty. Please provide sender via UI or configure DefaultSender in appsettings.json");
            }

            // Ensure recipient field is always present and valid
            if (string.IsNullOrEmpty(message.Destinations[0].To))
            {
                _logger.LogError("? CRITICAL: Recipient field is missing! This will cause API call to fail.");
                throw new InvalidOperationException("Recipient field cannot be null or empty. Please provide recipient via UI.");
            }

            // Ensure content.body is not null
            if (message.Content?.Body == null)
            {
                _logger.LogError("? CRITICAL: Content.Body is null! This will cause API call to fail.");
                // Create a default body to prevent API failure
                message.Content = message.Content ?? new InfobipContent();
                message.Content.Body = new InfobipBody
                {
                    Text = activity.Text ?? "Message",
                    Type = InfobipMessageTypes.Text
                };
                _logger.LogWarning($"?? Created default content.body to prevent API failure");
            }

            _logger.LogInformation("? Successfully converted Activity to Infobip Messages API request");
            
            // Log the final request structure for debugging
            try
            {
                var requestJson = JsonConvert.SerializeObject(request, Formatting.Indented);
                _logger.LogDebug($"?? Final request structure:\n{requestJson}");

                // Also print to console for source application debugging
                Console.WriteLine("=== CONVERTED INFOBIP MESSAGE REQUEST ===");
                Console.WriteLine($"Channel: {message.Channel}");
                Console.WriteLine($"Sender: {message.Sender}");
                Console.WriteLine($"Destination: {recipient}");
                Console.WriteLine($"Message ID: {message.MessageId}");
                Console.WriteLine($"Content.Body Present: {message.Content?.Body != null}");
                Console.WriteLine($"Content.Body.Type: {message.Content?.Body?.Type}");
                Console.WriteLine($"Content.Body.Text: {message.Content?.Body?.Text}");
                Console.WriteLine($"Request JSON:\n{requestJson}");
                Console.WriteLine("=========================================");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to serialize request for logging");
            }

            return Task.FromResult(request);
        }

        /// <summary>
        /// Get recipient with priority system: UI input > conversation override > original conversation ID
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <param name="originalConversationId">Original conversation ID</param>
        /// <returns>Recipient phone number or ID</returns>
        private string GetRecipientWithPriority(Activity activity, string originalConversationId)
        {
            // Priority 1: Custom recipient from UI input (when conversation ID was overridden)
            // This happens when the bot sets turnContext.Activity.Conversation.Id = customRecipient
            if (!string.IsNullOrEmpty(originalConversationId) && 
                originalConversationId.StartsWith("+") && 
                originalConversationId.Length >= 7)
            {
                _logger.LogInformation($"?? Using custom recipient from UI (phone number): {originalConversationId}");
                return originalConversationId;
            }

            // Priority 2: Check if conversation ID looks like a phone number (international format)
            if (!string.IsNullOrEmpty(originalConversationId) && 
                originalConversationId.StartsWith("+") && 
                System.Text.RegularExpressions.Regex.IsMatch(originalConversationId, @"^\+\d{7,15}$"))
            {
                _logger.LogInformation($"?? Using conversation ID as phone number: {originalConversationId}");
                return originalConversationId;
            }

            // Priority 3: Check for custom recipient in activity properties
            if (activity.Properties != null && activity.Properties["customRecipient"] != null)
            {
                var customRecipient = activity.Properties["customRecipient"].ToString();
                if (!string.IsNullOrEmpty(customRecipient))
                {
                    _logger.LogInformation($"?? Using custom recipient from activity properties: {customRecipient}");
                    return customRecipient;
                }
            }

            // Priority 4: Use original conversation ID as fallback
            _logger.LogInformation($"?? Using original conversation ID as recipient: {originalConversationId}");
            return originalConversationId;
        }

        /// <summary>
        /// Get sender from activity or configuration with proper priority
        /// Priority: 1. Custom sender from UI (activity properties), 2. Default configuration (appsettings)
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <returns>Sender ID</returns>
        private string GetSenderFromActivity(Activity activity)
        {
            // Priority 1: Check for custom sender from UI input (activity properties)
            if (activity.Properties != null && activity.Properties["customSender"] != null)
            {
                var customSender = activity.Properties["customSender"].ToString();
                if (!string.IsNullOrEmpty(customSender))
                {
                    _logger.LogInformation($"?? Using custom sender from UI: {customSender}");
                    return customSender;
                }
            }

            // Priority 2: Check for sender in channel data (programmatic override)
            if (activity.ChannelData != null)
            {
                try
                {
                    var channelDataObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        JsonConvert.SerializeObject(activity.ChannelData));
                    
                    if (channelDataObj.ContainsKey("sender"))
                    {
                        var sender = channelDataObj["sender"]?.ToString();
                        if (!string.IsNullOrEmpty(sender))
                        {
                            _logger.LogInformation($"?? Using sender from channel data: {sender}");
                            return sender;
                        }
                    }

                    if (channelDataObj.ContainsKey("from"))
                    {
                        var sender = channelDataObj["from"]?.ToString();
                        if (!string.IsNullOrEmpty(sender))
                        {
                            _logger.LogInformation($"?? Using from field from channel data: {sender}");
                            return sender;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse channel data for sender information");
                }
            }

            // Priority 3: Use configured default sender from appsettings.json
            if (!string.IsNullOrEmpty(_adapterOptions.DefaultSender))
            {
                _logger.LogInformation($"?? Using configured default sender from appsettings: {_adapterOptions.DefaultSender}");
                return _adapterOptions.DefaultSender;
            }

            // Priority 4: Use recipient ID from activity (bot ID) as fallback
            if (!string.IsNullOrEmpty(activity.Recipient?.Id))
            {
                _logger.LogInformation($"?? Using activity recipient ID as sender fallback: {activity.Recipient.Id}");
                return activity.Recipient.Id;
            }

            // If no sender is found, this will cause the API call to fail
            _logger.LogError("? No sender found! Provide sender via UI or configure DefaultSender in appsettings.json");
            return null;
        }

        /// <summary>
        /// Get channel from activity
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <returns>Infobip channel name</returns>
        private string GetChannelFromActivity(Activity activity)
        {
            // Priority 1: Check for explicit channel specification in entities
            var channelSpec = activity.GetInfobipChannelSpecification();
            if (!string.IsNullOrEmpty(channelSpec?.PreferredChannel))
            {
                var mappedChannel = MapToInfobipChannel(channelSpec.PreferredChannel);
                _logger.LogInformation($"?? Channel detected from channel specification: {mappedChannel}");
                return mappedChannel;
            }

            // Priority 2: Check channel data (multiple possible keys)
            if (activity.ChannelData != null)
            {
                try
                {
                    var channelDataObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        JsonConvert.SerializeObject(activity.ChannelData));
                    
                    // Check for infobipChannel key
                    if (channelDataObj.ContainsKey("infobipChannel"))
                    {
                        var mappedChannel = MapToInfobipChannel(channelDataObj["infobipChannel"]?.ToString());
                        _logger.LogInformation($"?? Channel detected from channel data (infobipChannel): {mappedChannel}");
                        return mappedChannel;
                    }

                    // Check for generic channel key
                    if (channelDataObj.ContainsKey("channel"))
                    {
                        var mappedChannel = MapToInfobipChannel(channelDataObj["channel"]?.ToString());
                        _logger.LogInformation($"?? Channel detected from channel data (channel): {mappedChannel}");
                        return mappedChannel;
                    }

                    // Check for Messages API channel key
                    if (channelDataObj.ContainsKey("messagesChannel"))
                    {
                        var mappedChannel = MapToInfobipChannel(channelDataObj["messagesChannel"]?.ToString());
                        _logger.LogInformation($"?? Channel detected from channel data (messagesChannel): {mappedChannel}");
                        return mappedChannel;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse channel data for channel specification");
                }
            }

            // Priority 3: Check Bot Framework ChannelId mapping
            if (_adapterOptions.EnableAutomaticChannelDetection && !string.IsNullOrEmpty(activity.ChannelId))
            {
                var mappedChannel = MapToInfobipChannel(activity.ChannelId);
                _logger.LogInformation($"?? Channel detected from Bot Framework ChannelId: {activity.ChannelId} -> {mappedChannel}");
                return mappedChannel;
            }

            // Priority 4: Use default channel from configuration
            var defaultChannel = _adapterOptions.DefaultChannel ?? InfobipChannels.WhatsApp;
            _logger.LogInformation($"?? Using default channel: {defaultChannel}");
            return defaultChannel;
        }

        /// <summary>
        /// Map channel identifier to Infobip channel name
        /// </summary>
        /// <param name="channel">Channel identifier</param>
        /// <returns>Infobip channel name</returns>
        private string MapToInfobipChannel(string channel)
        {
            if (string.IsNullOrEmpty(channel)) return null;

            switch (channel.ToUpperInvariant())
            {
                case "WHATSAPP":
                case "WHATSAPP_BUSINESS":
                    return InfobipChannels.WhatsApp;
                case "SMS":
                case "TEXT":
                case "TWILIO_SMS":
                    return InfobipChannels.SMS;
                case "MMS":
                case "MULTIMEDIA":
                    return InfobipChannels.MMS;
                case "VIBER":
                case "VIBER_BM":
                case "VIBER_BUSINESS":
                    return InfobipChannels.ViberBM;
                case "VIBER_BOT":
                case "VIBER_PUBLIC":
                    return InfobipChannels.ViberBot;
                case "RCS":
                case "RICH_COMMUNICATION":
                    return InfobipChannels.RCS;
                case "APPLE_MB":
                case "APPLE_BUSINESS":
                case "IMESSAGE":
                    return InfobipChannels.AppleMB;
                case "INSTAGRAM_DM":
                case "INSTAGRAM":
                    return InfobipChannels.InstagramDM;
                case "LINE_ON":
                case "LINE":
                    return InfobipChannels.LineON;
                case "MESSENGER":
                case "FACEBOOK":
                case "FACEBOOK_MESSENGER":
                    return InfobipChannels.Messenger;
                case "GOOGLE_BM":
                case "GOOGLE_BUSINESS":
                case "GOOGLE_BUSINESS_MESSAGES":
                    return InfobipChannels.GoogleBM;
                case "TELEGRAM":
                    return InfobipChannels.Telegram;
                case "EMAIL":
                case "MAIL":
                    return InfobipChannels.Email;
                case "VOICE":
                case "CALL":
                case "PHONE":
                    return InfobipChannels.Voice;
                case "PUSH":
                case "PUSH_NOTIFICATION":
                    return InfobipChannels.Push;
                default:
                    _logger.LogWarning($"?? Unknown channel '{channel}', using WhatsApp as fallback");
                    return InfobipChannels.WhatsApp; // Default fallback
            }
        }

        /// <summary>
        /// Convert Bot Framework activity to Infobip content
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <returns>Infobip content</returns>
        private InfobipContent ConvertActivityToContent(Activity activity)
        {
            var content = new InfobipContent();

            // Handle text message - ALWAYS create a body even if empty
            if (!string.IsNullOrEmpty(activity.Text))
            {
                content.Body = new InfobipBody
                {
                    Text = activity.Text,
                    Type = InfobipMessageTypes.Text
                };
                _logger.LogInformation($"?? Created text body: '{activity.Text}'");
            }

            // Handle media attachments
            if (activity.Attachments?.Any() == true)
            {
                ProcessAttachments(activity, content);
            }

            // Handle suggested actions as buttons
            if (activity.SuggestedActions?.Actions?.Any() == true)
            {
                ProcessSuggestedActions(activity, content);
            }

            // Handle choice prompts as lists
            if (IsChoicePrompt(activity))
            {
                ProcessChoicePromptAsList(activity, content);
            }

            // Handle location entities
            ProcessLocationEntities(activity, content);

            // Handle template data from channel data
            ProcessTemplateData(activity, content);

            // CRITICAL: If no content body was set, create a default one
            if (content.Body == null)
            {
                content.Body = new InfobipBody
                {
                    Text = activity.Text ?? "Message",
                    Type = InfobipMessageTypes.Text
                };
                _logger.LogWarning($"?? No body content found, created default text body: '{content.Body.Text}'");
            }

            _logger.LogInformation($"? Content conversion complete. Body type: {content.Body.Type}, Text: '{content.Body.Text}'");
            return content;
        }

        /// <summary>
        /// Process media attachments
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <param name="content">Infobip content to update</param>
        private void ProcessAttachments(Activity activity, InfobipContent content)
        {
            var firstAttachment = activity.Attachments.First();

            // For document attachments
            if (IsDocumentAttachment(firstAttachment))
            {
                content.Body = new InfobipBody
                {
                    Url = firstAttachment.ContentUrl,
                    Text = firstAttachment.Name ?? activity.Text ?? "Document",
                    Type = InfobipMessageTypes.Document
                };
            }
        }

        /// <summary>
        /// Check if attachment is a document
        /// </summary>
        /// <param name="attachment">Attachment to check</param>
        /// <returns>True if document attachment</returns>
        private bool IsDocumentAttachment(Attachment attachment)
        {
            if (string.IsNullOrEmpty(attachment.ContentType)) return false;

            var contentType = attachment.ContentType.ToLowerInvariant();
            return contentType.Contains("pdf") || 
                   contentType.Contains("doc") || 
                   contentType.Contains("application/");
        }

        /// <summary>
        /// Process suggested actions as buttons
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <param name="content">Infobip content to update</param>
        private void ProcessSuggestedActions(Activity activity, InfobipContent content)
        {
            if (activity.SuggestedActions.Actions.Count() <= 3 // WhatsApp limit
                && !activity.SuggestedActions.Actions.Any(a => string.IsNullOrEmpty(a.Title)))
            {
                content.Buttons = activity.SuggestedActions.Actions.Select(action => new InfobipButton
                {
                    Text = action.Title,
                    PostbackData = action.Value?.ToString() ?? action.Title,
                    Type = InfobipButtonTypes.QuickReply
                }).ToArray();

                _logger.LogInformation($"?? Processed {content.Buttons.Length} suggested actions as buttons");
            }
            else
            {
                _logger.LogWarning($"?? Skipped processing suggested actions as buttons (count={activity.SuggestedActions.Actions.Count()}; empty titles={activity.SuggestedActions.Actions.Count(a => string.IsNullOrEmpty(a.Title))})");
            }
        }

        /// <summary>
        /// Check if activity represents a choice prompt
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <returns>True if choice prompt</returns>
        private bool IsChoicePrompt(Activity activity)
        {
            return activity.SuggestedActions?.Actions?.Count() > 3;
        }

        /// <summary>
        /// Process choice prompt as list
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <param name="content">Infobip content to update</param>
        private void ProcessChoicePromptAsList(Activity activity, InfobipContent content)
        {
            content.Body = new InfobipBody
            {
                Text = activity.Text ?? "Please choose an option:",
                Subtext = "Select from the options below",
                Type = InfobipMessageTypes.List,
                Sections = new[]
                {
                    new InfobipSection
                    {
                        SectionTitle = "Options",
                        Items = activity.SuggestedActions.Actions.Select(action => new InfobipListItem
                        {
                            Id = action.Value?.ToString() ?? action.Title,
                            Text = action.Title,
                            Description = action.Text
                        }).ToArray()
                    }
                }
            };

            _logger.LogInformation($"?? Processed choice prompt as list with {content.Body.Sections.First().Items.Length} items");
        }

        /// <summary>
        /// Process location entities
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <param name="content">Infobip content to update</param>
        private void ProcessLocationEntities(Activity activity, InfobipContent content)
        {
            var geoEntity = activity.Entities?.FirstOrDefault(e => e.Type == "GeoCoordinates");
            if (geoEntity != null)
            {
                var geoCoordinates = geoEntity.GetAs<GeoCoordinates>();
                content.Body = new InfobipBody
                {
                    Latitude = geoCoordinates.Latitude,
                    Longitude = geoCoordinates.Longitude,
                    Name = geoCoordinates.Name,
                    Address = activity.Text,
                    Type = InfobipMessageTypes.Location
                };

                _logger.LogInformation($"?? Processed location entity: {geoCoordinates.Name} ({geoCoordinates.Latitude}, {geoCoordinates.Longitude})");
            }
        }

        /// <summary>
        /// Process template data from channel data
        /// </summary>
        /// <param name="activity">Bot Framework activity</param>
        /// <param name="content">Infobip content to update</param>
        private void ProcessTemplateData(Activity activity, InfobipContent content)
        {
            if (activity.ChannelData != null)
            {
                try
                {
                    var channelDataObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        JsonConvert.SerializeObject(activity.ChannelData));

                    // Check for template data
                    if (channelDataObj.ContainsKey("templateName"))
                    {
                        // This would be handled by a different message type in the actual API
                        // For now, we'll store it as a special body type
                        content.Body = new InfobipBody
                        {
                            Text = channelDataObj["templateName"]?.ToString(),
                            Type = "TEMPLATE"
                        };

                        _logger.LogInformation($"?? Processed template data: {channelDataObj["templateName"]}");
                    }

                    // Check for list data
                    if (channelDataObj.ContainsKey("listTitle") && channelDataObj.ContainsKey("listSections"))
                    {
                        var sections = JsonConvert.DeserializeObject<InfobipSection[]>
                            (JsonConvert.SerializeObject(channelDataObj["listSections"]));

                        content.Body = new InfobipBody
                        {
                            Text = channelDataObj["listTitle"]?.ToString(),
                            Type = InfobipMessageTypes.List,
                            Sections = sections
                        };

                        _logger.LogInformation($"?? Processed list data: {channelDataObj["listTitle"]} with {sections.Length} sections");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process template data from channel data");
                }
            }
        }
    }
}