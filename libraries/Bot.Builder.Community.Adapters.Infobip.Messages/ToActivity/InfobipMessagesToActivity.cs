using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.ToActivity
{
    public static class InfobipMessagesToActivity
    {
        /// <summary>
        /// Converts Infobip Messages API incoming message to Bot Framework activity
        /// </summary>
        /// <param name="result">Infobip incoming message result</param>
        /// <param name="client">Infobip Messages client</param>
        /// <returns>Bot Framework activity</returns>
        public static async Task<Activity> Convert(InfobipMessagesIncomingResult result, IInfobipMessagesClient client)
        {
            if (result?.Message == null) return null;

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Id = result.MessageId,
                Timestamp = result.ReceivedAt,
                ChannelId = InfobipMessagesConstants.ChannelId,
                From = new ChannelAccount
                {
                    Id = result.From,
                    Name = result.Contact?.Name ?? result.Contact?.Profile?.Name
                },
                Recipient = new ChannelAccount
                {
                    Id = result.To
                },
                Conversation = new ConversationAccount
                {
                    Id = result.From,
                    IsGroup = false
                },
                ChannelData = new
                {
                    Channel = result.Channel,
                    Platform = result.Platform,
                    Contact = result.Contact,
                    CallbackData = result.CallbackData,
                    SeenAt = result.SeenAt,
                    IsSeenReport = result.IsSeenReport(),
                    IsMessage = result.IsMessage()
                }
            };

            await ProcessMessageContent(result.Message, activity, client).ConfigureAwait(false);

            return activity;
        }

        private static async Task ProcessMessageContent(InfobipMessagesIncomingMessage message, Activity activity, IInfobipMessagesClient client)
        {
            switch (message.Type?.ToLower())
            {
                case InfobipMessagesMessageTypes.Text:
                    ProcessTextMessage(message, activity);
                    break;

                case InfobipMessagesMessageTypes.Image:
                case InfobipMessagesMessageTypes.Document:
                case InfobipMessagesMessageTypes.Video:
                case InfobipMessagesMessageTypes.Audio:
                case InfobipMessagesMessageTypes.Sticker:
                    await ProcessMediaMessage(message, activity, client).ConfigureAwait(false);
                    break;

                case InfobipMessagesMessageTypes.Location:
                    ProcessLocationMessage(message, activity);
                    break;

                case InfobipMessagesMessageTypes.Contact:
                    ProcessContactMessage(message, activity);
                    break;

                case InfobipMessagesMessageTypes.Interactive:
                    ProcessInteractiveMessage(message, activity);
                    break;

                case InfobipMessagesMessageTypes.Button:
                    ProcessButtonMessage(message, activity);
                    break;

                case InfobipMessagesMessageTypes.List:
                    ProcessListMessage(message, activity);
                    break;

                case InfobipMessagesMessageTypes.Flow:
                    ProcessFlowMessage(message, activity);
                    break;

                case InfobipMessagesMessageTypes.Template:
                    ProcessTemplateMessage(message, activity);
                    break;

                default:
                    ProcessUnsupportedMessage(message, activity);
                    break;
            }
        }

        private static void ProcessTextMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            activity.Text = message.Text ?? string.Empty;
            activity.TextFormat = TextFormatTypes.Plain;
        }

        private static async Task ProcessMediaMessage(InfobipMessagesIncomingMessage message, Activity activity, IInfobipMessagesClient client)
        {
            activity.Text = message.Caption ?? string.Empty;
            activity.TextFormat = TextFormatTypes.Plain;

            if (!string.IsNullOrEmpty(message.Url))
            {
                var attachment = new Attachment
                {
                    ContentType = GetContentTypeFromMessageType(message.Type),
                    ContentUrl = message.Url,
                    Name = message.Caption
                };

                // Try to get more specific content type and download content if needed
                try
                {
                    var downloadedAttachment = await client.GetAttachmentAsync(message.Url).ConfigureAwait(false);
                    if (downloadedAttachment != null)
                    {
                        attachment.Content = downloadedAttachment.Content;
                        attachment.ContentType = downloadedAttachment.ContentType ?? attachment.ContentType;
                        
                        // For stickers, add specific metadata
                        if (message.Type?.ToLower() == InfobipMessagesMessageTypes.Sticker)
                        {
                            activity.Entities = activity.Entities ?? new List<Entity>();
                            activity.Entities.Add(new Entity
                            {
                                Type = InfobipEntityTypes.StickerContent,
                                Properties = JObject.FromObject(new
                                {
                                    url = message.Url,
                                    caption = message.Caption
                                })
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    // If we can't download the attachment, just keep the URL
                }

                activity.Attachments = new List<Attachment> { attachment };
            }
        }

        private static void ProcessLocationMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            if (message.Location != null)
            {
                activity.Text = $"?? {message.Location.Name ?? "Location shared"}";
                if (!string.IsNullOrEmpty(message.Location.Address))
                {
                    activity.Text += $"\n{message.Location.Address}";
                }

                activity.Entities = new List<Entity>
                {
                    new GeoCoordinates
                    {
                        Latitude = message.Location.Latitude,
                        Longitude = message.Location.Longitude,
                        Name = message.Location.Name,
                        Type = "GeoCoordinates"
                    }
                };

                // Add location entity with additional metadata
                activity.Entities.Add(new Entity
                {
                    Type = InfobipEntityTypes.LocationContent,
                    Properties = JObject.FromObject(new
                    {
                        latitude = message.Location.Latitude,
                        longitude = message.Location.Longitude,
                        name = message.Location.Name,
                        address = message.Location.Address
                    })
                });
            }
        }

        private static void ProcessContactMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            if (message.ContactInfo != null)
            {
                var contactName = message.ContactInfo.Name?.FormattedName ?? 
                                 $"{message.ContactInfo.Name?.FirstName} {message.ContactInfo.Name?.LastName}".Trim() ?? 
                                 "Contact";
                
                activity.Text = $"?? Contact: {contactName}";
                activity.Entities = new List<Entity>();

                // Add contact information as entities
                if (message.ContactInfo.Phones?.Length > 0)
                {
                    foreach (var phone in message.ContactInfo.Phones)
                    {
                        var serializer = new JsonSerializer();
                        activity.Entities.Add(new Entity
                        {
                            Type = "phoneNumber",
                            Properties = JObject.FromObject(new
                            {
                                number = phone.Phone,
                                type = phone.Type ?? InfobipContactTypes.Mobile
                            }, serializer)
                        });
                    }
                }

                if (message.ContactInfo.Emails?.Length > 0)
                {
                    foreach (var email in message.ContactInfo.Emails)
                    {
                        var serializer = new JsonSerializer();
                        activity.Entities.Add(new Entity
                        {
                            Type = "email",
                            Properties = JObject.FromObject(new
                            {
                                email = email.Email,
                                type = email.Type ?? InfobipContactTypes.Work
                            }, serializer)
                        });
                    }
                }

                // Add full contact entity
                activity.Entities.Add(new Entity
                {
                    Type = InfobipEntityTypes.ContactContent,
                    Properties = JObject.FromObject(message.ContactInfo)
                });
            }
        }

        private static void ProcessInteractiveMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            if (message.Interactive?.ButtonReply != null)
            {
                activity.Text = message.Interactive.ButtonReply.Title;
                activity.Value = new
                {
                    type = "buttonReply",
                    id = message.Interactive.ButtonReply.Id,
                    title = message.Interactive.ButtonReply.Title
                };

                // Add interactive entity for additional processing
                activity.Entities = activity.Entities ?? new List<Entity>();
                activity.Entities.Add(new Entity
                {
                    Type = InfobipEntityTypes.InteractiveContent,
                    Properties = JObject.FromObject(new
                    {
                        interactionType = "button",
                        buttonId = message.Interactive.ButtonReply.Id,
                        buttonTitle = message.Interactive.ButtonReply.Title
                    })
                });
            }
            else if (message.Interactive?.ListReply != null)
            {
                activity.Text = message.Interactive.ListReply.Title;
                activity.Value = new
                {
                    type = "listReply",
                    id = message.Interactive.ListReply.Id,
                    title = message.Interactive.ListReply.Title,
                    description = message.Interactive.ListReply.Description
                };

                // Add interactive entity for additional processing
                activity.Entities = activity.Entities ?? new List<Entity>();
                activity.Entities.Add(new Entity
                {
                    Type = InfobipEntityTypes.InteractiveContent,
                    Properties = JObject.FromObject(new
                    {
                        interactionType = "list",
                        listItemId = message.Interactive.ListReply.Id,
                        listItemTitle = message.Interactive.ListReply.Title,
                        listItemDescription = message.Interactive.ListReply.Description
                    })
                });
            }
        }

        private static void ProcessButtonMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            if (message.Button != null)
            {
                activity.Text = message.Button.Title;
                activity.Value = new
                {
                    type = "buttonReply",
                    id = message.Button.Id,
                    title = message.Button.Title
                };

                activity.Entities = activity.Entities ?? new List<Entity>();
                activity.Entities.Add(new Entity
                {
                    Type = InfobipEntityTypes.InteractiveContent,
                    Properties = JObject.FromObject(new
                    {
                        interactionType = "button",
                        buttonId = message.Button.Id,
                        buttonTitle = message.Button.Title
                    })
                });
            }
        }

        private static void ProcessListMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            if (message.ListReply != null)
            {
                activity.Text = message.ListReply.Title;
                activity.Value = new
                {
                    type = "listReply",
                    id = message.ListReply.Id,
                    title = message.ListReply.Title,
                    description = message.ListReply.Description
                };

                activity.Entities = activity.Entities ?? new List<Entity>();
                activity.Entities.Add(new Entity
                {
                    Type = InfobipEntityTypes.InteractiveContent,
                    Properties = JObject.FromObject(new
                    {
                        interactionType = "list",
                        listItemId = message.ListReply.Id,
                        listItemTitle = message.ListReply.Title,
                        listItemDescription = message.ListReply.Description
                    })
                });
            }
        }

        private static void ProcessFlowMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            // WhatsApp Flow response handling
            activity.Text = "Flow interaction completed";
            activity.Value = new
            {
                type = "flowReply",
                flowData = message.Text // Flow data would be in text field
            };

            activity.Entities = activity.Entities ?? new List<Entity>();
            activity.Entities.Add(new Entity
            {
                Type = InfobipEntityTypes.FlowContent,
                Properties = JObject.FromObject(new
                {
                    interactionType = "flow",
                    flowData = message.Text
                })
            });
        }

        private static void ProcessTemplateMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            // Template message response (if any)
            activity.Text = message.Text ?? "Template message received";
            
            activity.Entities = activity.Entities ?? new List<Entity>();
            activity.Entities.Add(new Entity
            {
                Type = InfobipEntityTypes.TemplateContent,
                Properties = JObject.FromObject(new
                {
                    messageType = "template",
                    content = message.Text
                })
            });
        }

        private static void ProcessUnsupportedMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            activity.Text = message.Text ?? $"?? Unsupported message type: {message.Type}";
            activity.TextFormat = TextFormatTypes.Plain;
            
            // Log the unsupported message type for debugging
            activity.Entities = activity.Entities ?? new List<Entity>();
            activity.Entities.Add(new Entity
            {
                Type = "unsupportedMessage",
                Properties = JObject.FromObject(new
                {
                    messageType = message.Type,
                    originalContent = message.Text,
                    url = message.Url,
                    caption = message.Caption
                })
            });
        }

        private static string GetContentTypeFromMessageType(string messageType)
        {
            if (string.IsNullOrEmpty(messageType))
                return "application/octet-stream";

            switch (messageType.ToLower())
            {
                case InfobipMessagesMessageTypes.Image:
                    return "image/*";
                case InfobipMessagesMessageTypes.Video:
                    return "video/*";
                case InfobipMessagesMessageTypes.Audio:
                    return "audio/*";
                case InfobipMessagesMessageTypes.Document:
                    return "application/*";
                case InfobipMessagesMessageTypes.Sticker:
                    return "image/webp"; // WhatsApp stickers are typically WebP
                default:
                    return "application/octet-stream";
            }
        }
    }
}