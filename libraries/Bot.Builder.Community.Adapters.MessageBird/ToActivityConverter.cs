using Bot.Builder.Community.Adapters.MessageBird.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Adapters.MessageBird
{
    public class ToActivityConverter
    {
        private readonly ILogger _logger;
        private readonly MessageBirdAdapterOptions _adapterOptions;

        public ToActivityConverter(MessageBirdAdapterOptions adapterOptions, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _adapterOptions = adapterOptions ?? throw new ArgumentNullException(nameof(adapterOptions));
        }

        public List<Activity> Convert(MessageBirdWebhookPayload messageBirdIncomingMessage)
        {
            if (messageBirdIncomingMessage == null) throw new ArgumentNullException(nameof(messageBirdIncomingMessage));
            var result = new List<Activity>();
            try
            {
                var activity = ConvertToActivity(messageBirdIncomingMessage);
                if (activity != null)
                    result.Add(activity);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, "Error handling message response: " + e.Message, e);

            }
            return result;
        }

        private Activity ConvertToActivity(MessageBirdWebhookPayload response)
        {
            if (response.message.direction.ToLower() == "sent")//after delivery
            {
                var activity = new Activity
                {
                    Type = ActivityTypes.Event,
                    Id = response.message.id,
                    ChannelId = $"{response.message.platform}-{response.message.channelId}",
                    ChannelData = response,
                    Recipient = new ChannelAccount { Id = response.message.to },
                    From = new ChannelAccount { Id = response.message.from ?? "you" },
                    Conversation = new ConversationAccount { IsGroup = false, Id = response.conversation.Id },
                    Timestamp = response.message.createdDatetime,
                    Text = null,
                    Name = "delivery",
                    Entities = new List<Entity>()
                };
                return activity;
            }

            if (response.message.direction == "received")//incoming message
            {
                return (Activity)ConvertMessageToMessageActivity(response);
            }

            throw new Exception("Unsupported message received - not DLR, SEEN or MO message: \n" +
                                JsonConvert.SerializeObject(response, Formatting.Indented));
        }

        private IActivity ConvertMessageToMessageActivity(MessageBirdWebhookPayload response)
        {
            var activity = Activity.CreateMessageActivity();

            activity.Type = ActivityTypes.Message;
            activity.Id = response.message.id;
            activity.ChannelId = $"{response.message.platform}-{response.message.channelId}";
            activity.ChannelData = response;
            activity.Recipient = new ChannelAccount { Id = response.message.to };
            activity.From = new ChannelAccount { Id = response.message.from };
            activity.Conversation = new ConversationAccount { IsGroup = false, Id = response.conversation.Id };
            activity.Timestamp = response.message.createdDatetime;

            switch (response.message.type)
            {
                case "audio":
                    {
                        activity.Attachments = new List<Attachment>
                        {
                            new Attachment
                            {
                                ContentType = "audio",
                                ContentUrl = response.message.content.Audio.Url,
                                Name = response.message.content.Audio.Caption ?? ""
                            }
                        };
                        break;
                    }
                case "file":
                    {
                        activity.Attachments = new List<Attachment>
                        {
                            new Attachment
                            {
                                ContentType = "file",
                                ContentUrl = response.message.content.File.Url,
                                Name = response.message.content.File.Caption ?? ""
                            }
                        };
                        break;
                    }
                case "image":
                    {
                        activity.Attachments = new List<Attachment>
                        {
                            new Attachment
                            {
                                ContentType = "image",
                                ContentUrl = response.message.content.Image.Url,
                                Name = response.message.content.Image.Caption ?? ""
                            }
                        };
                        break;
                    }
                case "location":
                    {
                        activity.Entities.Add(new GeoCoordinates
                        {
                            Latitude = response.message.content.Location.Latitude,
                            Longitude = response.message.content.Location.Longitude
                        });
                        break;
                    }
                case "text":
                    {
                        activity.Text = response.message.content.Text;
                        activity.TextFormat = TextFormatTypes.Plain;
                        break;
                    }
                case "video":
                    {
                        activity.Attachments = new List<Attachment>
                        {
                            new Attachment
                            {
                                ContentType = "video",
                                ContentUrl = response.message.content.Video.Url,
                                Name = response.message.content.Video.Caption ?? ""
                            }
                        };
                        break;
                    }
                //this will be addes as soon as MessageBird nuget package add support for this message type, my PR is waiting to be merged
                //case "whatsappSticker":
                //    {
                //        activity.Attachments = new List<Attachment>
                //        {
                //            new Attachment
                //            {
                //                ContentType = "whatsappSticker",
                //                ContentUrl = response.message.content.WhatsAppSticker.Link,
                //                Name = ""
                //            }
                //        };
                //        break;
                //    }
                default:
                    {
                        return null;
                    }
            }

            return activity;
        }
    }
}
