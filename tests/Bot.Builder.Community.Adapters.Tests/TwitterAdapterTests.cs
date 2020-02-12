using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;
using Castle.Core.Internal;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bot.Builder.Community.Adapters.Twitter.Tests
{
    [TestClass]
    [TestCategory("Twitter")]
    public class TwitterAdapterTests
    {
        private readonly Mock<IOptions<TwitterOptions>> _testOptions = new Mock<IOptions<TwitterOptions>>();

        [TestMethod]
        public void ConstructorWithOptionsSucceeds()
        {
            Assert.IsNotNull(new TwitterAdapter(_testOptions.Object));
        }

        [TestMethod]
        public void UseWithMiddlewareShouldSucceed()
        {
            var adapter = new TwitterAdapter(_testOptions.Object);
            var middleware = new Mock<IMiddleware>();
            var result = adapter.Use(middleware.Object);

            Assert.IsFalse(result.MiddlewareSet.IsNullOrEmpty());
        }

        [TestMethod]
        public async Task ProcessActivityShouldSucceed()
        {
            var adapter = new TwitterAdapter(_testOptions.Object);
            var directMessageEvent = new DirectMessageEvent();
            var bot = new Mock<IBot>();

            directMessageEvent.MessageText = "test message text";
            directMessageEvent.Sender = new TwitterUser();
            directMessageEvent.Recipient = new TwitterUser();
            directMessageEvent.Sender.Id = string.Empty;
            directMessageEvent.Sender.ScreenName = string.Empty;
            directMessageEvent.Recipient.Id = string.Empty;
            directMessageEvent.Recipient.ScreenName = string.Empty;
            bot.SetupAllProperties();

            await adapter.ProcessActivity(directMessageEvent, bot.Object.OnTurnAsync);
            bot.Verify(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ProcessActivityShouldReturnNullReferenceException()
        {
            var adapter = new TwitterAdapter(_testOptions.Object);

            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
            {
                await adapter.ProcessActivity(null, null);
            });
        }

        [TestMethod]
        public async Task SendActivitiesAsyncShouldReturnEmptyResponsesWithEmptyActivities()
        {
            var adapter = new TwitterAdapter(_testOptions.Object);
            var activity = new Activity();

            using (var turnContext = new TurnContext(adapter, activity))
            {
                var result = await adapter.SendActivitiesAsync(turnContext, new Activity[0], default);
                Assert.IsTrue(result.IsNullOrEmpty());
            }
        }

        [TestMethod]
        public async Task UpdateActivityAsyncShouldReturnNotSupportedException()
        {
            var adapter = new TwitterAdapter(_testOptions.Object);
            var activity = new Activity();
            
            using (var turnContext = new TurnContext(adapter, activity))
            {
                await Assert.ThrowsExceptionAsync<NotSupportedException>(async () =>
                {
                   await adapter.UpdateActivityAsync(turnContext, activity, default);
                });
            }
        }

        [TestMethod]
        public async Task DeleteActivityAsyncShouldReturnNotSupportedException()
        {
            var adapter = new TwitterAdapter(_testOptions.Object);
            var activity = new Activity();
            var conversationReference = new ConversationReference();

            using (var turnContext = new TurnContext(adapter, activity))
            {
                await Assert.ThrowsExceptionAsync<NotSupportedException>(async () =>
                {
                    await adapter.DeleteActivityAsync(turnContext, conversationReference, default);
                });
            }
        }
    }
}
