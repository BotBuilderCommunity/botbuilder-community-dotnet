using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.ToActivity;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Tests.ToActivityTests
{
    public class InfobipMessagesToActivityTests
    {
        private Mock<IInfobipMessagesClient> _infobipClient;
        private const string _contentType = "image/*";

        public InfobipMessagesToActivityTests()
        {
            _infobipClient = new Mock<IInfobipMessagesClient>(MockBehavior.Strict);
            _infobipClient.Setup(x => x.GetContentTypeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(_contentType);
            _infobipClient.Setup(x => x.GetAttachmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Attachment)null);
        }

        [Fact]
        public async Task ConvertMessagesTextMessageToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipMessagesIncomingResult>
            {
                Results = new List<InfobipMessagesIncomingResult>
                {
                    new InfobipMessagesIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "messages-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "MESSAGES",
                        Channel = "whatsapp",
                        Platform = "whatsapp",
                        Message = new InfobipMessagesIncomingMessage
                        {
                            Type = InfobipMessagesMessageTypes.Text,
                            Text = "Text message to bot"
                        },
                        Contact = new InfobipMessagesContact
                        {
                            Name = "Subscriber Name",
                            Profile = new InfobipMessagesContactProfile
                            {
                                Name = "Profile Name"
                            }
                        },
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        },
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = await InfobipMessagesToActivity.Convert(incomingMessage.Results.Single(), _infobipClient.Object).ConfigureAwait(false);

            Assert.NotNull(activity);
            Assert.Equal(InfobipMessagesConstants.ChannelId, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultTextMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertMessagesImageMessageToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipMessagesIncomingResult>
            {
                Results = new List<InfobipMessagesIncomingResult>
                {
                    new InfobipMessagesIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "messages-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "MESSAGES",
                        Channel = "whatsapp",
                        Platform = "whatsapp",
                        Message = new InfobipMessagesIncomingMessage
                        {
                            Caption = "Message Caption",
                            Type = InfobipMessagesMessageTypes.Image,
                            Url = "https://infobip.api.media.endpoint"
                        },
                        Contact = new InfobipMessagesContact
                        {
                            Name = "Subscriber Name"
                        },
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        },
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = await InfobipMessagesToActivity.Convert(incomingMessage.Results.Single(), _infobipClient.Object).ConfigureAwait(false);

            Assert.NotNull(activity);
            Assert.Equal(InfobipMessagesConstants.ChannelId, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultImageMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertMessagesLocationMessageToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipMessagesIncomingResult>
            {
                Results = new List<InfobipMessagesIncomingResult>
                {
                    new InfobipMessagesIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "messages-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "MESSAGES",
                        Channel = "whatsapp",
                        Platform = "whatsapp",
                        Message = new InfobipMessagesIncomingMessage
                        {
                            Type = InfobipMessagesMessageTypes.Location,
                            Location = new InfobipMessagesLocation
                            {
                                Name = "Location Name",
                                Address = "Location Address",
                                Latitude = 45.793365478515625,
                                Longitude = 15.9459228515625
                            }
                        },
                        Contact = new InfobipMessagesContact
                        {
                            Name = "Subscriber Name"
                        },
                        Price = new InfobipIncomingPrice
                        {
                            PricePerMessage = 0,
                            Currency = "GBP"
                        },
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = await InfobipMessagesToActivity.Convert(incomingMessage.Results.Single(), _infobipClient.Object).ConfigureAwait(false);

            Assert.NotNull(activity);
            Assert.Equal(InfobipMessagesConstants.ChannelId, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultLocationMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertMessagesInteractiveMessageToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipMessagesIncomingResult>
            {
                Results = new List<InfobipMessagesIncomingResult>
                {
                    new InfobipMessagesIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "messages-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "MESSAGES",
                        Channel = "whatsapp",
                        Platform = "whatsapp",
                        Message = new InfobipMessagesIncomingMessage
                        {
                            Type = InfobipMessagesMessageTypes.Interactive,
                            Interactive = new InfobipMessagesInteractive
                            {
                                ButtonReply = new InfobipMessagesButtonReply
                                {
                                    Id = "button-1",
                                    Title = "Button Title"
                                }
                            }
                        },
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = await InfobipMessagesToActivity.Convert(incomingMessage.Results.Single(), _infobipClient.Object).ConfigureAwait(false);

            Assert.NotNull(activity);
            Assert.Equal(InfobipMessagesConstants.ChannelId, activity.ChannelId);

            VerifyResultCoreProperties(incomingMessage.Results[0], activity);
            VerifyResultInteractiveMessage(incomingMessage.Results[0].Message, activity);
        }

        [Fact]
        public async Task ConvertMessagesUnsupportedMessageTypeToActivity()
        {
            var incomingMessage = new InfobipIncomingMessage<InfobipMessagesIncomingResult>
            {
                Results = new List<InfobipMessagesIncomingResult>
                {
                    new InfobipMessagesIncomingResult
                    {
                        MessageId = "Unique message Id",
                        From = "subscriber-number",
                        To = "messages-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "MESSAGES",
                        Channel = "whatsapp",
                        Platform = "whatsapp",
                        Message = new InfobipMessagesIncomingMessage
                        {
                            Type = "UNSUPPORTED",
                        },
                    }
                },
                MessageCount = 1,
                PendingMessageCount = 0
            };

            var activity = await InfobipMessagesToActivity.Convert(incomingMessage.Results.Single(), _infobipClient.Object).ConfigureAwait(false);

            Assert.NotNull(activity);
            Assert.Equal("?? Unsupported message type: UNSUPPORTED", activity.Text);
        }

        private void VerifyResultCoreProperties(InfobipIncomingResultBase result, Activity activity)
        {
            Assert.Equal(result.MessageId, activity.Id);
            Assert.Equal(result.From, activity.From.Id);
            Assert.Equal(result.To, activity.Recipient.Id);
            Assert.Equal(result.From, activity.Conversation.Id);
            Assert.Equal(result.ReceivedAt, activity.Timestamp);
        }

        private void VerifyResultTextMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(message.Text, activity.Text);

            Assert.True(activity.Attachments == null || activity.Attachments.Count == 0);
        }

        private void VerifyResultImageMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);

            Assert.NotNull(activity.Attachments);
            Assert.Equal(1, activity.Attachments.Count);

            var attachment = activity.Attachments[0];

            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(message.Url, attachment.ContentUrl);
            Assert.Equal(message.Caption, attachment.Name);
            Assert.Equal(_contentType, attachment.ContentType);

            Assert.Equal(message.Caption, activity.Text);
        }

        private void VerifyResultLocationMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);

            // Accept 1 or more entities, but check at least one is GeoCoordinates
            Assert.True(activity.Entities.Count >= 1);
            var entity = activity.Entities.FirstOrDefault(e => e.GetAs<GeoCoordinates>() != null)?.GetAs<GeoCoordinates>();
            Assert.NotNull(entity);
            Assert.Equal(message.Location.Longitude, entity.Longitude);
            Assert.Equal(message.Location.Latitude, entity.Latitude);
            Assert.Equal(message.Location.Name, entity.Name);
        }

        private void VerifyResultInteractiveMessage(InfobipMessagesIncomingMessage message, Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(message.Interactive.ButtonReply.Title, activity.Text);
            Assert.NotNull(activity.Value);
        }
    }
}