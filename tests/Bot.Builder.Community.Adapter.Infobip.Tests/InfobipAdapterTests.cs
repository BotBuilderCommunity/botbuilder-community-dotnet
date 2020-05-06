using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapter.Infobip.Tests.Framework;
using Bot.Builder.Community.Adapters.Infobip;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Bot.Builder.Community.Adapter.Infobip.Tests
{
    [TestFixture]
    public class InfobipAdapterTests
    {
        [Test]
        public async Task SendActivities_NoActivities()
        {
            var adapter = GetInfobipAdapter();
            var turnContext = new Mock<ITurnContext>(MockBehavior.Strict);

            // No activities.
            var activities = new Activity[] { };

            var responses = await adapter.SendActivitiesAsync(turnContext.Object, activities, CancellationToken.None).ConfigureAwait(false);

            // No responses.
            Assert.AreEqual(0, responses.Length);
        }

        [Test]
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

        [Test]
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
            Assert.AreEqual(1, responses.Length);
            Assert.AreEqual(messageId.ToString(), responses[0].Id);
        }

        [Test]
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

        [Test]
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
                    Assert.AreEqual(messageId.ToString(), response.Id);
                }
            };

            await adapter.ProcessAsync(httpContext.Request, httpContext.Response, bot, CancellationToken.None).ConfigureAwait(false);

            // Bot was called 1x.
            Assert.AreEqual(1, bot.OnMessageActivityInvocationCount);
        }

        [Test]
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
                    Assert.AreEqual(messageIds.Length, responses.Count);
                    for (int i = 0; i < messageIds.Length; ++i)
                        Assert.AreEqual(messageIds[i].Value.ToString(), responses[i].Id);
                }
            };

            await adapter.ProcessAsync(httpContext.Request, httpContext.Response, bot, CancellationToken.None).ConfigureAwait(false);

            // Bot was called 1x.
            Assert.AreEqual(1, bot.OnMessageActivityInvocationCount);
        }

        [Test]
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
                    Assert.AreEqual(messageIds.Length, responses.Length);
                    for (int i = 0; i < messageIds.Length; ++i)
                        Assert.AreEqual(messageIds[i].Value.ToString(), responses[i].Id);
                }
            };

            await adapter.ProcessAsync(httpContext.Request, httpContext.Response, bot, CancellationToken.None).ConfigureAwait(false);

            // Bot was called 1x.
            Assert.AreEqual(1, bot.OnMessageActivityInvocationCount);
        }

        [Test]
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

        [Test]
        public async Task Process_OneActivityWithMultipleInfobipMessages()
        {
            var messageIds = new Guid?[] { Guid.NewGuid(), Guid.NewGuid() };
            var adapter = GetInfobipAdapter(messageIds);
            var activity = CreateMessageActivity();
            // A message with attachments will create two messages to Infobip.
            activity.Attachments.Add(new Attachment{ContentUrl = "http://dummy-image", ContentType = "image"});
            activity.Attachments.Add(new Attachment { ContentUrl = "http://dummy-image", ContentType = "image" });

            var responses = await adapter.SendActivitiesAsync(new TurnContext(adapter, CreateMessageActivity()),
                new [] {activity}, CancellationToken.None);
            var infobipResponses = responses.Select(x => x as InfobipResourceResponse).ToArray();

            Assert.AreEqual(infobipResponses.Length, 1);
            var infobipResponse = infobipResponses.First();
            Assert.NotNull(infobipResponse);
            Assert.AreEqual(string.Join("|", messageIds), infobipResponse.Id);
            Assert.AreEqual(infobipResponse.ActivityId, activity.Id);
            Assert.AreEqual(infobipResponse.ResponseMessages.Count, 2);
            Assert.IsTrue(infobipResponse.ResponseMessages.Any(x => x.MessageId == messageIds[0]));
            Assert.IsTrue(infobipResponse.ResponseMessages.Any(x => x.MessageId == messageIds[1]));
        }

        [Test]
        public async Task Process_TwoActivitiesWithMultipleInfobipMessages()
        {
            var messageIds = new Guid?[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var adapter = GetInfobipAdapter(messageIds);
            var activities = new List<Activity>();
            var firstActivity = CreateMessageActivity();
            activities.Add(firstActivity);
            var secondActivity = CreateMessageActivity();
            secondActivity.Attachments = new List<Attachment>
            {
                new Attachment {ContentUrl = "http://dummy-video", ContentType = "video"},
                new Attachment {ContentUrl = "http://dummy-image", ContentType = "image"}
            };
            activities.Add(secondActivity);

            var responses = await adapter.SendActivitiesAsync(new TurnContext(adapter, CreateMessageActivity()),
                activities.ToArray(), CancellationToken.None);
            var infobipResponses = responses.Select(x => x as InfobipResourceResponse).ToArray();

            Assert.AreEqual(infobipResponses.Length, 2);
            var firstInfobipResponse = infobipResponses.First();
            Assert.AreEqual(messageIds[0].ToString(), firstInfobipResponse.Id);
            Assert.AreEqual(firstInfobipResponse.ActivityId, firstActivity.Id);
            Assert.AreEqual(firstInfobipResponse.ResponseMessages.Count, 1);
            Assert.IsTrue(firstInfobipResponse.ResponseMessages[0].MessageId == messageIds[0]);

            var secondResponse = infobipResponses.ElementAt(1);
            Assert.AreEqual(string.Join("|", new []{messageIds[1], messageIds[2]}), secondResponse.Id);
            Assert.AreEqual(secondResponse.ActivityId, secondActivity.Id);
            Assert.AreEqual(secondResponse.ResponseMessages.Count, 2);
            Assert.IsTrue(secondResponse.ResponseMessages[0].MessageId == messageIds[1]);
            Assert.IsTrue(secondResponse.ResponseMessages[1].MessageId == messageIds[2]);
        }

        public Activity CreateMessageActivity()
        {
            var message = MessageFactory.Text("text for message");
            message.Id = Guid.NewGuid().ToString();
            message.Recipient = new ChannelAccount { Id = "whatsapp-number" };

            return message;
        }

        private InfobipAdapter GetInfobipAdapter(params Guid?[] messageIds)
        {
            Func<InfobipResponseMessage> throwException = () => throw new HttpRequestException();
            IEnumerable<Func<InfobipResponseMessage>> clientResponses = messageIds.Select(x => x.HasValue ? () => new InfobipResponseMessage { MessageId = x.Value } : throwException);

            var options = new InfobipAdapterOptions("apiKey", "apiBaseUrl", "appSecret", "whatsAppNumber", "scenarioKey");
            options.BypassAuthentication = true;

            var client = new Mock<IInfobipClient>(MockBehavior.Strict);

            var queue = new Queue<Func<InfobipResponseMessage>>(clientResponses);
            client.Setup(x => x.SendMessageAsync(It.IsAny<InfobipOmniFailoverMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InfobipOmniFailoverMessage x, CancellationToken y) => { return queue.Dequeue()(); });

            return new InfobipAdapter(options, client.Object, NullLogger<InfobipAdapter>.Instance);
        }

        private InfobipIncomingMessage GetInfobipIncomingMessage()
        {
            return new InfobipIncomingMessage
            {
                Results = new List<InfobipIncomingResult>
                {
                    new InfobipIncomingResult
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
