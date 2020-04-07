using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.RingCentral.Handoff;
using Bot.Builder.Community.Adapters.RingCentral.Tests.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Moq;
using RingCentral.EngageDigital.SourceSdk;
using Xunit;
using static Bot.Builder.Community.Adapters.RingCentral.RingCentralConstants;

namespace Bot.Builder.Community.Adapters.RingCentral.Tests
{
    public class RingCentralClientWrapperTests
    {
        private const string PROPERAPIACCESSKEYFORMANUALTESTS = "NeedsAdjustments";
        private const string PROPERENGAGEURLFORMANUALTEST = "NeedsAdjustments";

        [Fact]
        public async Task VerifyWebhookAsync_ValidTokenAndChallengeCode_ReturnsChallengeCodeBodyAnd200()
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            var response = new DefaultHttpContext().Response;
            response.Body = new MemoryStream();

            string webhookChallengeCode = "5af96abed0b2fc27a017bf5a7e961dd9";
            string webhookToken = "abc123";
            request.Setup(x => x.QueryString).Returns(QueryString.FromUriComponent($"?hub.challenge={webhookChallengeCode}&hub.mode=subscribe&hub.verify_token={webhookToken}"));

            var mockOptions = new RingCentralOptions() { RingCentralEngageApiAccessToken = "abc", RingCentralEngageApiUrl = "http://localhost", BotId = "testbot", RingCentralEngageWebhookValidationToken = webhookToken };
            var sut = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());

            // Act
            await sut.VerifyWebhookAsync(request.Object, response, It.IsAny<CancellationToken>());
            response.Body.Position = 0;
            using var sr = new StreamReader(response.Body);
            var responseBody = sr.ReadToEnd();

            // Assert
            Assert.Equal(webhookChallengeCode, responseBody);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }

        [Fact]
        public async Task VerifyWebhookAsync_InvalidToken_Returns401()
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            var response = new Mock<HttpResponse>();

            string webhookChallengeCode = "5af96abed0b2fc27a017bf5a7e961dd9";
            string webhookToken = "abc123";
            request.Setup(x => x.QueryString).Returns(QueryString.FromUriComponent($"?hub.challenge={webhookChallengeCode}&hub.mode=subscribe&hub.verify_token={webhookToken}"));
            response.Setup(x => x.Body).Returns(new MemoryStream());

            var mockOptions = new RingCentralOptions() { RingCentralEngageApiAccessToken = "abc", RingCentralEngageApiUrl = "http://localhost", BotId = "testbot", RingCentralEngageWebhookValidationToken = "incorrect_token" };
            var sut = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());

            // Act
            await sut.VerifyWebhookAsync(request.Object, response.Object, It.IsAny<CancellationToken>());
            response.Object.Body.Position = 0;
            using var sr = new StreamReader(response.Object.Body);
            var responseBody = sr.ReadToEnd();

            // Assert
            Assert.NotEqual(webhookChallengeCode, responseBody);
            response.VerifySet(x => x.StatusCode = StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task VerifyWebhookAsync_MissingHubChallenge_ReturnsBadRequest()
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            var response = new Mock<HttpResponse>();

            string webhookChallengeCode = "5af96abed0b2fc27a017bf5a7e961dd9";
            string webhookToken = "abc123";
            request.Setup(x => x.QueryString).Returns(QueryString.FromUriComponent($"?hub.mode=subscribe&hub.verify_token={webhookToken}"));
            response.Setup(x => x.Body).Returns(new MemoryStream());

            var mockOptions = new RingCentralOptions() { RingCentralEngageApiAccessToken = "abc", RingCentralEngageApiUrl = "http://localhost", BotId = "testbot", RingCentralEngageWebhookValidationToken = "incorrect_token" };
            var sut = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());

            // Act
            await sut.VerifyWebhookAsync(request.Object, response.Object, It.IsAny<CancellationToken>());
            response.Object.Body.Position = 0;
            using var sr = new StreamReader(response.Object.Body);
            var responseBody = sr.ReadToEnd();

            // Assert
            Assert.NotEqual(webhookChallengeCode, responseBody);
            response.VerifySet(x => x.StatusCode = StatusCodes.Status400BadRequest);
        }

        [Theory]
        [InlineData("ActionMessageCreate.json", "hey")]
        public async Task GetActivityFromRingCentralRequestAsync_ActionRequest_ReturnsActionAndActivityText(string jsonFile, string expectedMessage)
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            var response = new DefaultHttpContext().Response;
            response.Body = new MemoryStream();
            var ringCentralRequest = GetEmbeddedTestData($"{GetType().Namespace}.TestData.{jsonFile}");
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            sw.Write(ringCentralRequest);
            sw.Flush();
            ms.Position = 0;
            request.Setup(x => x.Body).Returns(ms);
            request.Setup(x => x.Path).Returns(PathString.FromUriComponent("/action"));

            var mockOptions = new RingCentralOptions() { RingCentralEngageApiAccessToken = "abc", RingCentralEngageApiUrl = "http://localhost", BotId = "testbot", MicrosoftAppId = "appId" };
            var sut = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());

            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>();
            mockBotAdapter.As<IAdapterIntegration>().Setup(x => x.ContinueConversationAsync(mockOptions.BotId, new ConversationReference(), null, default)).Returns(Task.CompletedTask);
            mockBotAdapter.Verify();
            var adapter = new RingCentralAdapter(sut, mockBotAdapter.Object, new StaticHandoffRequestRecognizer());

            // Act
            var (ringCentralWebhook, activity) = await sut.GetActivityFromRingCentralRequestAsync(adapter, mockBotAdapter.Object, request.Object, response);

            // Assert
            Assert.Equal(RingCentralHandledEvent.Action, ringCentralWebhook);
            Assert.Equal(expectedMessage, activity.Text);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }

        /// <summary>
        /// To properly test this unitest the input json file and the engage options need to get properly adjusted.
        /// The input json file can be compared completed with the informations gained through the Postman setup.
        /// </summary>
        /// <param name="jsonFile">Real json payload.</param>
        /// <param name="expectedMessage">Message text from the WhatsApp platform.</param>
        /// <returns>Task.</returns>
        [Theory]
        [InlineData("WhatsAppContentImported.json", "hi")]
        public async Task GetActivityFromRingCentralRequestAsync_WhatsAppRequest_ReturnsContentImported(string jsonFile, string expectedMessage)
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            var response = new DefaultHttpContext().Response;
            response.Body = new MemoryStream();
            var ringCentralRequest = GetEmbeddedTestData($"{GetType().Namespace}.TestData.{jsonFile}");
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            sw.Write(ringCentralRequest);
            sw.Flush();
            ms.Position = 0;
            request.Setup(x => x.Body).Returns(ms);
            request.Setup(x => x.Path).Returns(PathString.FromUriComponent("/contentimport"));

            var mockOptions = new RingCentralOptions()
            { 
                RingCentralEngageApiAccessToken = PROPERAPIACCESSKEYFORMANUALTESTS,
                RingCentralEngageApiUrl = PROPERENGAGEURLFORMANUALTEST,
                BotId = "testbot",
                RingCentralEngageBotControlledThreadCategoryId = "botcontrolled"
            };
            var sut = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var adapter = new RingCentralAdapter(sut, mockBotAdapter, new StaticHandoffRequestRecognizer());

            // Act
            var (ringCentralWebhook, activity) = await sut.GetActivityFromRingCentralRequestAsync(adapter, mockBotAdapter, request.Object, response);

            // Assert
            Assert.Equal(RingCentralHandledEvent.ContentImported, ringCentralWebhook);
            Assert.Equal(expectedMessage, activity.Text);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }

        [Theory]
        [InlineData("UnspecificSourceContentImported.json")]
        public async Task GetActivityFromRingCentralRequestAsync_UnspecificSourceRequest_ReturnsContentImportedAndUnspecificChannelId(string jsonFile)
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            var response = new DefaultHttpContext().Response;
            response.Body = new MemoryStream();
            var ringCentralRequest = GetEmbeddedTestData($"{GetType().Namespace}.TestData.{jsonFile}");
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            sw.Write(ringCentralRequest);
            sw.Flush();
            ms.Position = 0;
            request.Setup(x => x.Body).Returns(ms);
            request.Setup(x => x.Path).Returns(PathString.FromUriComponent("/contentimport"));

            var mockOptions = new RingCentralOptions() { RingCentralEngageApiAccessToken = "abc", RingCentralEngageApiUrl = "http://localhost", BotId = "testbot", RingCentralEngageBotControlledThreadCategoryId = "botcontrolled" };
            var mockRingCentralClient = new Mock<DimeloClient>();

            var sut = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var adapter = new RingCentralAdapter(sut, mockBotAdapter, new StaticHandoffRequestRecognizer());

            // Act
            var (ringCentralWebhook, activity) = await sut.GetActivityFromRingCentralRequestAsync(adapter, mockBotAdapter, request.Object, response);

            // Assert
            Assert.Equal(RingCentralHandledEvent.ContentImported, ringCentralWebhook);
            Assert.Equal(RingCentralChannels.Unspecific, activity.ChannelId);
        }

        [Theory]
        [InlineData("WhatsAppContentImportedNoBody.json")]
        public async Task GetActivityFromRingCentralRequestAsync_WhatsAppContentImportedMissingBodyText_ReturnsUnknownEventAndNullActivity(string jsonFile)
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            var response = new Mock<HttpResponse>();
            var ringCentralRequest = GetEmbeddedTestData($"{GetType().Namespace}.TestData.{jsonFile}");
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            sw.Write(ringCentralRequest);
            sw.Flush();
            ms.Position = 0;
            request.Setup(x => x.Body).Returns(ms);
            request.Setup(x => x.Path).Returns(PathString.FromUriComponent("/contentimport"));
            response.Setup(x => x.Body).Returns(new MemoryStream());

            var mockOptions = new RingCentralOptions() { RingCentralEngageApiAccessToken = "abc", RingCentralEngageApiUrl = "http://localhost", BotId = "testbot", RingCentralEngageBotControlledThreadCategoryId = "botcontrolled" };
            var mockRingCentralClient = new Mock<DimeloClient>();

            var sut = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var adapter = new RingCentralAdapter(sut, mockBotAdapter, new StaticHandoffRequestRecognizer());

            // Act
            var (ringCentralWebhook, activity) = await sut.GetActivityFromRingCentralRequestAsync(adapter, mockBotAdapter, request.Object, response.Object);

            // Assert
            Assert.Equal(RingCentralHandledEvent.Unknown, ringCentralWebhook);
            Assert.Null(activity);
            response.VerifySet(x => x.StatusCode = StatusCodes.Status204NoContent);
        }

        [Theory]
        [InlineData("WhatsAppContentImportedAgentRequest.json", "human")]
        public async Task GetActivityFromRingCentralWhatsAppContentImportedEventRequestForAgent(string jsonFile, string expectedMessage)
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            var response = new DefaultHttpContext().Response;
            response.Body = new MemoryStream();
            var ringCentralRequest = GetEmbeddedTestData($"{GetType().Namespace}.TestData.{jsonFile}");
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            sw.Write(ringCentralRequest);
            sw.Flush();
            ms.Position = 0;
            request.Setup(x => x.Body).Returns(ms);
            request.Setup(x => x.Path).Returns(PathString.FromUriComponent("/contentimport"));

            var mockOptions = new RingCentralOptions() { RingCentralEngageApiAccessToken = "abc", RingCentralEngageApiUrl = "http://localhost", BotId = "testbot", RingCentralEngageBotControlledThreadCategoryId = "botcontrolled" };
            var mockRingCentralClient = new Mock<DimeloClient>();

            var sut = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var adapter = new RingCentralAdapter(sut, mockBotAdapter, new StaticHandoffRequestRecognizer());

            // Act
            var (ringCentralWebhook, activity) = await sut.GetActivityFromRingCentralRequestAsync(adapter, mockBotAdapter, request.Object, response);

            // Assert
            Assert.Equal(RingCentralHandledEvent.ContentImported, ringCentralWebhook);
            Assert.Equal(expectedMessage, activity.Text);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }

        [Theory]
        [InlineData("ImplementationInfoRequest.json")]
        public async Task GetActivityFromRingCentralRequestAsync_ImplementationInfoRequest_ReturnsActionAndNullActivity(string jsonFile)
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            var response = new DefaultHttpContext().Response;
            response.Body = new MemoryStream();
            var ringCentralRequest = GetEmbeddedTestData($"{GetType().Namespace}.TestData.{jsonFile}");
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            sw.Write(ringCentralRequest);
            sw.Flush();
            ms.Position = 0;
            request.Setup(x => x.Body).Returns(ms);
            request.Setup(x => x.Path).Returns(PathString.FromUriComponent("/action"));

            var mockOptions = new RingCentralOptions() { RingCentralEngageApiAccessToken = "abc", RingCentralEngageApiUrl = "http://localhost", BotId = "testbot" };
            var sut = new RingCentralClientWrapper(OptionsHelper.GetOptionsMonitor(mockOptions), new StaticHandoffRequestRecognizer());
            var mockBotAdapter = new Mock<IBotFrameworkHttpAdapter>().Object;
            var adapter = new RingCentralAdapter(sut, mockBotAdapter, new StaticHandoffRequestRecognizer());

            // Act
            var (ringCentralWebhook, activity) = await sut.GetActivityFromRingCentralRequestAsync(adapter, mockBotAdapter, request.Object, response);

            // Assert
            Assert.Equal(RingCentralHandledEvent.Action, ringCentralWebhook);
            Assert.Null(activity);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }

        /// <summary>
        /// Loads the embedded json resource with the LUIS as a string.
        /// </summary>
        private string GetEmbeddedTestData(string resourceName)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
