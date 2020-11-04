using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.Tests.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp.Tests
{
    public class InfobipAdapterTests
    {
        [Fact]
        public async Task SendActivities_NoActivities()
        {
            var adapter = GetInfobipAdapter();
            var turnContext = new Mock<ITurnContext>(MockBehavior.Strict);

            // No activities.
            var activities = new Activity[] { };

            var responses = await adapter.SendActivitiesAsync(turnContext.Object, activities, CancellationToken.None).ConfigureAwait(false);

            // No responses.
            Assert.Empty(responses);
        }

        [Fact]
        public void SendActivities_BadActivity()
        {
            var adapter = GetInfobipAdapter();
            var turnContext = new Mock<ITurnContext>(MockBehavior.Strict);

            var badActivity = CreateMessageActivity();
            badActivity.Recipient = null;   // Recipient is a required field.

            var activities = new Activity[]
            {
                badActivity
            };

            // Throws a validation exception.
            Assert.ThrowsAsync<Microsoft.Rest.ValidationException>(async () => await adapter.SendActivitiesAsync(turnContext.Object, activities, CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task SendActivities_SingleActivity_SendSucceeds()
        {
            // Setup client to return a success message.
            var messageId = Guid.NewGuid();
            var adapter = GetInfobipAdapter(messageId);
            var turnContext = new Mock<ITurnContext>(MockBehavior.Strict);

            // Single activity
            var activities = new Activity[]
            {
                CreateMessageActivity()
            };

            var responses = await adapter.SendActivitiesAsync(turnContext.Object, activities, CancellationToken.None).ConfigureAwait(false);

            // Singe response with the message id configured above.
            Assert.Single(responses);
            Assert.Equal(messageId.ToString(), responses[0].Id);
        }

        [Fact]
        public void SendActivities_MultipleActivitiesOneFails()
        {
            // Setup client to throw for the 2nd message sent (1st and 3rd work fine).
            var messageIds = new Guid?[] { Guid.NewGuid(), null, Guid.NewGuid() };
            var adapter = GetInfobipAdapter(messageIds);
            var turnContext = new Mock<ITurnContext>(MockBehavior.Strict);

            var activities = new Activity[]
            {
                CreateMessageActivity(),
                CreateMessageActivity(),
                CreateMessageActivity()
            };

            // Send activities fails with the exception configured.
            Assert.ThrowsAsync<HttpRequestException>(async () => await adapter.SendActivitiesAsync(turnContext.Object, activities, CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task Process_SingleMessage()
        {
            // Setup client to return a success message.
            var messageId = Guid.NewGuid();
            var adapter = GetInfobipAdapter(messageId);

            // Add a message to the incoming request.
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetInfobipIncomingMessage())));

            var bot = new TestBot
            {
                OnMessageActivity = async (_, t, c) =>
                {
                    // Configure the bot to reply to the message with a single activity.
                    var activity = CreateMessageActivity();
                    var response = await t.SendActivityAsync(activity, c).ConfigureAwait(false);

                    // Sending succeeds and bot gets the message id configured above.
                    Assert.Equal(messageId.ToString(), response.Id);
                }
            };

            await adapter.ProcessAsync(httpContext.Request, httpContext.Response, bot, CancellationToken.None).ConfigureAwait(false);

            // Bot was called 1x.
            Assert.Equal(1, bot.OnMessageActivityInvocationCount);
        }

        [Fact]
        public async Task Process_MultipleSeparateMessages()
        {
            // Setup client to return two success results.
            var messageIds = new Guid?[] { Guid.NewGuid(), Guid.NewGuid() };
            var adapter = GetInfobipAdapter(messageIds);

            // Add a message to the incoming request.
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetInfobipIncomingMessage())));

            var bot = new TestBot
            {
                OnMessageActivity = async (x, t, c) =>
                {
                    // Bot responds with multiple messages separately.
                    var activity = CreateMessageActivity();
                    var responses = new List<ResourceResponse>();
                    for (int i = 0; i < messageIds.Length; ++i)
                        responses.Add(await t.SendActivityAsync(activity, c).ConfigureAwait(false));

                    // We got a response for each message and they are in the order we sent them.
                    Assert.Equal(messageIds.Length, responses.Count);
                    for (var i = 0; i < messageIds.Length; ++i)
                        Assert.Equal(messageIds[i].Value.ToString(), responses[i].Id);
                }
            };

            await adapter.ProcessAsync(httpContext.Request, httpContext.Response, bot, CancellationToken.None).ConfigureAwait(false);

            // Bot was called 1x.
            Assert.Equal(1, bot.OnMessageActivityInvocationCount);
        }

        [Fact]
        public async Task Process_MultipleMessagesTogether()
        {
            // Setup client to return two success results.
            var messageIds = new Guid?[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var adapter = GetInfobipAdapter(messageIds);

            // Add a message to the incoming request.
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetInfobipIncomingMessage())));

            var bot = new TestBot
            {
                OnMessageActivity = async (x, t, c) =>
                {
                    // Bot responds with multiple messages separately.
                    var responses = await t.SendActivitiesAsync(new[] { CreateMessageActivity(), CreateMessageActivity(), CreateMessageActivity() }, c).ConfigureAwait(false);

                    // We got a response for each message and they are in the order we sent them.
                    Assert.Equal(messageIds.Length, responses.Length);
                    for (var i = 0; i < messageIds.Length; ++i)
                        Assert.Equal(messageIds[i].Value.ToString(), responses[i].Id);
                }
            };

            await adapter.ProcessAsync(httpContext.Request, httpContext.Response, bot, CancellationToken.None).ConfigureAwait(false);

            // Bot was called 1x.
            Assert.Equal(1, bot.OnMessageActivityInvocationCount);
        }

        [Fact]
        public void Process_MultipleMessagesTogether_OneFails()
        {
            // Setup client to throw for the 2nd message sent (1st and 3rd work fine).
            var messageIds = new Guid?[] { Guid.NewGuid(), null, Guid.NewGuid() };
            var adapter = GetInfobipAdapter(messageIds);

            // Add a message to the incoming request.
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetInfobipIncomingMessage())));

            var bot = new TestBot
            {
                OnMessageActivity = async (x, t, c) =>
                {
                    // Bot responds with multiple messages separately.
                    var responses = await t.SendActivitiesAsync(new[] { CreateMessageActivity(), CreateMessageActivity(), CreateMessageActivity() }, c).ConfigureAwait(false);
                }
            };

            // Operation fails because send fails.
            Assert.ThrowsAsync<HttpRequestException>(async () => await adapter.ProcessAsync(httpContext.Request, httpContext.Response, bot, CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task Process_OneActivityWithMultipleInfobipMessages()
        {
            var messageIds = new Guid?[] { Guid.NewGuid(), Guid.NewGuid() };
            var adapter = GetInfobipAdapter(messageIds);
            var activity = CreateMessageActivity();
            activity.Attachments.Add(new Attachment { ContentUrl = "http://dummy-image", ContentType = "image" });
            activity.Attachments.Add(new Attachment { ContentUrl = "http://dummy-image2", ContentType = "image" });

            var responses = await adapter.SendActivitiesAsync(new TurnContext(adapter, CreateMessageActivity()),
                new[] { activity }, CancellationToken.None);
            var infobipResponses = responses.Select(x => x as InfobipResourceResponse).ToArray();

            Assert.Single(infobipResponses);
            var infobipResponse = infobipResponses.First();
            Assert.NotNull(infobipResponse);
            Assert.Equal(string.Join("|", messageIds), infobipResponse.Id);
            Assert.Equal(infobipResponse.ActivityId, activity.Id);
            Assert.Equal(2, infobipResponse.ResponseMessages.Count);
            Assert.Contains(infobipResponse.ResponseMessages, x => x.MessageId == messageIds[0]);
            Assert.Contains(infobipResponse.ResponseMessages, x => x.MessageId == messageIds[1]);
        }

        [Fact]
        public async Task Process_TwoActivitiesWithMultipleInfobipMessages()
        {
            var messageIds = new Guid?[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var adapter = GetInfobipAdapter(messageIds);
            var activities = new List<Activity>();
            var firstActivity = CreateMessageActivity();
            activities.Add(firstActivity);
            var secondActivity = CreateMessageActivity();
            secondActivity.Attachments = new List<Attachment>
            {
                new Attachment {ContentUrl = "http://dummy-video", ContentType = "video"},
                new Attachment {ContentUrl = "http://dummy-image", ContentType = "image"},
                new Attachment {ContentUrl = "http://dummy-image2", ContentType = "image"}
            };
            activities.Add(secondActivity);

            var responses = await adapter.SendActivitiesAsync(new TurnContext(adapter, CreateMessageActivity()),
                activities.ToArray(), CancellationToken.None);
            var infobipResponses = responses.Select(x => x as InfobipResourceResponse).ToArray();

            Assert.Equal(2, infobipResponses.Length);
            var firstInfobipResponse = infobipResponses.First();
            Assert.Equal(messageIds[0].ToString(), firstInfobipResponse.Id);
            Assert.Equal(firstInfobipResponse.ActivityId, firstActivity.Id);
            Assert.Single(firstInfobipResponse.ResponseMessages);
            Assert.True(firstInfobipResponse.ResponseMessages[0].MessageId == messageIds[0]);

            var secondResponse = infobipResponses.ElementAt(1);
            Assert.Equal(string.Join("|", new[] { messageIds[1], messageIds[2], messageIds[3] }), secondResponse.Id);
            Assert.Equal(secondResponse.ActivityId, secondActivity.Id);
            Assert.Equal(3, secondResponse.ResponseMessages.Count);
            Assert.True(secondResponse.ResponseMessages[0].MessageId == messageIds[1]);
            Assert.True(secondResponse.ResponseMessages[1].MessageId == messageIds[2]);
            Assert.True(secondResponse.ResponseMessages[2].MessageId == messageIds[3]);
        }

        private Activity CreateMessageActivity()
        {
            var message = MessageFactory.Text("text for message");
            message.Id = Guid.NewGuid().ToString();
            message.Recipient = new ChannelAccount { Id = "whatsapp-number" };
            message.ChannelId = InfobipWhatsAppConstants.ChannelName;

            return message;
        }

        private InfobipWhatsAppAdapter GetInfobipAdapter(params Guid?[] messageIds)
        {
            Func<InfobipResponseMessage> throwException = () => throw new HttpRequestException();
            IEnumerable<Func<InfobipResponseMessage>> clientResponses = messageIds.Select(x => x.HasValue ? () => new InfobipResponseMessage { MessageId = x.Value } : throwException);

            var options = new InfobipWhatsAppAdapterOptions("apiKey", "apiBaseUrl", "appSecret", "whatsAppNumber", "whatsAppScenarioKey");
            options.BypassAuthentication = true;

            var client = new Mock<IInfobipWhatsAppClient>(MockBehavior.Strict);

            var queue = new Queue<Func<InfobipResponseMessage>>(clientResponses);
            client.Setup(x => x.SendMessageAsync(It.IsAny<InfobipOmniFailoverMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InfobipOmniFailoverMessage x, CancellationToken y) => { return queue.Dequeue()(); });

            return new InfobipWhatsAppAdapter(options, client.Object, NullLogger<InfobipWhatsAppAdapter>.Instance);
        }

        private InfobipIncomingMessage<InfobipWhatsAppIncomingResult> GetInfobipIncomingMessage()
        {
            return new InfobipIncomingMessage<InfobipWhatsAppIncomingResult>
            {
                Results = new List<InfobipWhatsAppIncomingResult>
                {
                    new InfobipWhatsAppIncomingResult
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        From = "subscriber-number",
                        To = "whatsapp-number",
                        ReceivedAt = DateTimeOffset.UtcNow,
                        IntegrationType = "WHATSAPP",
                        Message = new InfobipIncomingWhatsAppMessage
                        {
                            Type = InfobipIncomingMessageTypes.Text,
                            Text = "Text message to bot"
                        },
                        Contact = new InfobipIncomingWhatsAppContact
                        {
                            Name = "Whatsapp Subscriber Name"
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
        }
    }
}
