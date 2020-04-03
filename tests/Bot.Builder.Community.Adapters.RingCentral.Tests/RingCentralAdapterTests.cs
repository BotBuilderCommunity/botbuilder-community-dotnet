using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.RingCentral.Handoff;
using Bot.Builder.Community.Adapters.RingCentral.Tests.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Moq;
using RingCentral;
using Xunit;

namespace Bot.Builder.Community.Adapters.RingCentral.Tests
{
    public class RingCentralAdapterTests
    {
        private readonly IOptionsMonitor<RingCentralOptions> _testOptions =
            OptionsHelper.GetOptionsMonitor(
                new RingCentralOptions() 
                { 
                    RingCentralEngageApiAccessToken = "abc", 
                    RingCentralEngageApiUrl = "http://localhost", 
                    BotId = "testbot" 
                });            

        [Fact]
        public void ConstructorWithAdapterApiWrapperSucceeds()
        {
            // Arrange
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var mockOptions = new RingCentralOptions()
            {
                RingCentralEngageApiAccessToken = "1111",
                RingCentralEngageApiUrl = "http://someurl.com",
                BotId = "testbot"
            };
            var rcw = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());

            // Act
            var sut = new RingCentralAdapter(rcw, mockBotAdapter, new StaticHandoffRequestRecognizer());
            
            // Assert
            Assert.NotNull(sut);
        }

        [Fact]
        public void ConstructorShouldFailWithNullRingCentralWrapper()
        {
            RingCentralClientWrapper rcw = null;
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            Assert.Throws<ArgumentNullException>(() => { new RingCentralAdapter(rcw, mockBotAdapter, new StaticHandoffRequestRecognizer()); });
        }

        [Fact]
        public void ConstructorShouldFailWithNullBotAdapter()
        {
            RingCentralClientWrapper rcw = null;
            IBotFrameworkHttpAdapter botAdapter = null;
            Assert.Throws<ArgumentNullException>(() => { new RingCentralAdapter(rcw, botAdapter, new StaticHandoffRequestRecognizer()); });
        }

        [Fact]
        public async void SendActivitiesAsyncShouldSucceed()
        {
            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Attachments = new List<Attachment> { new Attachment(contentUrl: "http://example.com") };
            activity.Object.Conversation = new ConversationAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";

            var turnContext = new Mock<ITurnContext>();
            turnContext.SetupGet(tc => tc.Activity).Returns(() => 
                new Activity()
                {
                    ChannelData = new RingCentralChannelData()
                    {
                        SourceId = "9f4bba850e69dc636a707fd6",
                        ThreadId = "a_thread_id"
                    }
                });

            const string resourceIdentifier = "ringCentralContentId";
            var mockOptions = new RingCentralOptions()
            {
                RingCentralEngageApiAccessToken = "1111",
                RingCentralEngageApiUrl = "http://someurl.com",
                BotId = "testbot"
            };

            var rcw = new Mock<RingCentralClientWrapper>(OptionsHelper.GetOptionsMonitor(mockOptions));
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            rcw.Setup(x => x.SendContentToRingCentralAsync(activity.Object, "9f4bba850e69dc636a707fd6")).Returns(Task.FromResult(resourceIdentifier));
            var adapter = new RingCentralAdapter(rcw.Object, mockBotAdapter, new StaticHandoffRequestRecognizer());

            var resourceResponses = await adapter.SendActivitiesAsync(turnContext.Object, new Activity[] { activity.Object }, default).ConfigureAwait(false);
            Assert.True(resourceResponses[0].Id == resourceIdentifier);
        }

        [Fact]
        public async void SendActivitiesAsyncShouldSucceedAndNoActivityReturnedWithActivityTypeNotMessage()
        {
            // Arrange
            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = ActivityTypes.Trace;
            activity.Object.Attachments = new List<Attachment> { new Attachment(contentUrl: "http://example.com") };
            activity.Object.Conversation = new ConversationAccount(id: "MockId");
            activity.Object.Text = "Trace content";

            const string resourceIdentifier = "ringCentralContentId";
            var mockOptions = new RingCentralOptions()
            {
                RingCentralEngageApiAccessToken = "1111",
                RingCentralEngageApiUrl = "http://someurl.com",
                BotId = "testbot"
            };
            
            var rcw = new Mock<RingCentralClientWrapper>(OptionsHelper.GetOptionsMonitor(mockOptions));
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            rcw.Setup(x => x.SendContentToRingCentralAsync(activity.Object, "9f4bba850e69dc636a707fd6")).Returns(Task.FromResult(resourceIdentifier));
            var adapter = new RingCentralAdapter(rcw.Object, mockBotAdapter, new StaticHandoffRequestRecognizer());

            // Act
            var resourceResponses = await adapter.SendActivitiesAsync(null, new Activity[] { activity.Object }, default).ConfigureAwait(false);
            
            // Assert
            Assert.True(resourceResponses.Length == 0);
        }

        [Fact]
        public async void ProcessAsyncShouldFailWithNullHttpRequest()
        {
            // Arrange
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var mockOptions = new RingCentralOptions()
            {
                RingCentralEngageApiAccessToken = "1111",
                RingCentralEngageApiUrl = "http://someurl.com",
                BotId = "testbot"
            };
            var rcw = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var adapter = new RingCentralAdapter(rcw, mockBotAdapter, new StaticHandoffRequestRecognizer());
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await adapter.ProcessAsync(null, httpResponse.Object, bot.Object, default);
            });
        }

        [Fact]
        public async void ProcessAsyncShouldFailWithNullHttpResponse()
        {
            // Arrange
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var mockOptions = new RingCentralOptions()
            {
                RingCentralEngageApiAccessToken = "1111",
                RingCentralEngageApiUrl = "http://someurl.com",
                BotId = "testbot"
            };
            var rcw = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var adapter = new RingCentralAdapter(rcw, mockBotAdapter, new StaticHandoffRequestRecognizer());
            var httpRequest = new Mock<HttpRequest>();
            var bot = new Mock<IBot>();
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await adapter.ProcessAsync(httpRequest.Object, null, bot.Object, default);
            });
        }

        [Fact]
        public async void ProcessAsyncShouldFailWithNullBot()
        {
            // Arrange
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var mockOptions = new RingCentralOptions()
            {
                RingCentralEngageApiAccessToken = "1111",
                RingCentralEngageApiUrl = "http://someurl.com",
                BotId = "testbot"
            };
            var rcw = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var adapter = new RingCentralAdapter(rcw, mockBotAdapter, new StaticHandoffRequestRecognizer());
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await adapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default);
            });
        }

        [Fact]
        public async void UpdateActivityAsyncShouldThrowNotSupportedException()
        {
            // Arrange
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var mockOptions = new RingCentralOptions()
            {
                RingCentralEngageApiAccessToken = "1111",
                RingCentralEngageApiUrl = "http://someurl.com",
                BotId = "testbot"
            };
            var rcw = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var adapter = new RingCentralAdapter(rcw, mockBotAdapter, new StaticHandoffRequestRecognizer());
            var activity = new Activity();
            
            using (var turnContext = new TurnContext(adapter, activity))
            {
                // Act & Assert
                await Assert.ThrowsAsync<NotImplementedException>(async () =>
                {
                    await adapter.UpdateActivityAsync(turnContext, activity, default);
                });
            }
        }

        [Fact]
        public async void DeleteActivityAsyncShouldThrowNotSupportedException()
        {
            // Arrange
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var mockOptions = new RingCentralOptions()
            {
                RingCentralEngageApiAccessToken = "1111",
                RingCentralEngageApiUrl = "http://someurl.com",
                BotId = "testbot"
            };
            var rcw = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var adapter = new RingCentralAdapter(rcw, mockBotAdapter, new StaticHandoffRequestRecognizer());
            var activity = new Activity();
            var conversationReference = new ConversationReference();
            
            // Act
            using (var turnContext = new TurnContext(adapter, activity))
            {
                // Assert
                await Assert.ThrowsAsync<NotImplementedException>(async () =>
                {
                    await adapter.DeleteActivityAsync(turnContext, conversationReference, default);
                });
            }
        }
    }
}
