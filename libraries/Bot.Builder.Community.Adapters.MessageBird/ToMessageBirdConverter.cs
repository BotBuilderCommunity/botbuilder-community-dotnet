using Bot.Builder.Community.Adapters.MessageBird.Models;
using MessageBird.Objects.Conversations;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Builder.Community.Adapters.MessageBird
{
    public class ToMessageBirdConverter
    {
        public static List<MessageBirdSendMessagePayload> Convert(Activity activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));

            var messages = new List<MessageBirdSendMessagePayload>();
            var message = new MessageBirdSendMessagePayload();

            if (!string.IsNullOrWhiteSpace(activity.Text))
                HandleText(activity, messages);

            if (activity.Attachments != null && activity.Attachments.Any())
            {
                HandleAttachments(activity, messages);
            }

            if (activity.Entities != null && activity.Entities.Any())
            {
                HandleEntities(activity, messages);
            }

            return messages;
        }
        private static void HandleText(IMessageActivity activity, IList<MessageBirdSendMessagePayload> messages)
        {
            var message = CreateMessage(activity);
            message.conversationMessageRequest.Content = new Content() { Text = activity.Text };
            message.conversationMessageRequest.Type = ContentType.Text;
            messages.Add(message);
        }
        private static void HandleAttachments(IMessageActivity activity, ICollection<MessageBirdSendMessagePayload> messages)
        {
            foreach (var attachment in activity.Attachments)
            {
                var message = CreateMessage(activity);

                var contentType = attachment.ContentType.ToLower();

                switch (contentType)
                {
                    case "image":
                        { 
                            message.conversationMessageRequest.Content = new Content() 
                            { 
                                Image = new MediaContent() 
                                { 
                                    Url = attachment.ContentUrl,
                                    Caption = attachment.Name ?? ""
                                } 
                            };
                            message.conversationMessageRequest.Type = ContentType.Image;
                            break;
                        }
                    case "video":
                        {
                            message.conversationMessageRequest.Content = new Content() 
                            { 
                                Video = new MediaContent() 
                                { 
                                    Url = attachment.ContentUrl,
                                    Caption = attachment.Name ?? ""
                                } 
                            };
                            message.conversationMessageRequest.Type = ContentType.Video;
                            break;
                        }
                    case "audio":
                        {
                            message.conversationMessageRequest.Content = new Content() 
                            { 
                                Audio = new MediaContent() 
                                { 
                                    Url = attachment.ContentUrl,
                                    Caption = attachment.Name ?? ""
                                } 
                            };
                            message.conversationMessageRequest.Type = ContentType.Audio;
                            break;
                        }
                    //case "whatsappsticker":
                    //    {
                    //        message.conversationMessageRequest.Content = new Content() 
                    //        { 
                    //            File = new WhatsAppStickerContent(
                    //            { 
                    //                Link = attachment.ContentUrl 
                    //            } 
                    //        };
                    //        message.conversationMessageRequest.Type = ContentType.WhatsAppSticker;
                    //        break;
                    //    }
                    default:
                        {
                            message.conversationMessageRequest.Content = new Content() 
                            { 
                                File = new MediaContent() 
                                { 
                                    Url = attachment.ContentUrl,
                                    Caption = attachment.Name ?? ""
                                } 
                            };
                            message.conversationMessageRequest.Type = ContentType.File;
                            break;
                        }
                }
                messages.Add(message);
            }
        }

        private static void HandleEntities(IMessageActivity activity, IList<MessageBirdSendMessagePayload> messages)
        {
            foreach (var entity in activity.Entities)
            {
                var location = entity.GetAs<GeoCoordinates>();
                if (location.Type == nameof(GeoCoordinates))
                {
                    var message = CreateMessage(activity);

                    message.conversationMessageRequest.Content = new Content() 
                    { 
                        Location = new LocationContent() 
                        { 
                            Latitude = (float)location.Latitude, 
                            Longitude = (float)location.Longitude 
                        } 
                    };
                    message.conversationMessageRequest.Type = ContentType.Location;

                    messages.Add(message);
                }
            }
        }

        private static MessageBirdSendMessagePayload CreateMessage(IMessageActivity activity)
        {
            return new MessageBirdSendMessagePayload()
            {
                conversationId = activity.Conversation.Id,
                conversationMessageRequest = new ConversationMessageSendRequest()
                {
                    ConversationId = activity.Conversation.Id,
                    ChannelId = activity.ChannelId.Split('#')[1]
                }
            };
        }
    }
}
