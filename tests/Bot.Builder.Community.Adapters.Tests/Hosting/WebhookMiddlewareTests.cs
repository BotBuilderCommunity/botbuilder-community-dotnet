using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bot.Builder.Community.Adapters.Twitter.Tests.Hosting
{
    [TestClass]
    [TestCategory("Twitter")]
    public class WebhookMiddlewareTests
    {
        private readonly Mock<ILogger<WebhookMiddleware>> _testLogger = new Mock<ILogger<WebhookMiddleware>>();
        private readonly Mock<IBot> _testBot = new Mock<IBot>();
        private readonly Mock<TwitterOptions> _twitterOptions = new Mock<TwitterOptions>() { Object = { ConsumerSecret = "ConsumerSecret" } };
        private readonly Mock<RequestDelegate> _testDelegate = new Mock<RequestDelegate>();
        private StringValues _values = new StringValues("test_code");

        [TestMethod]
        public async Task InvokeAsyncValidShouldSucceed()
        {
            var testOptions = new Mock<IOptions<TwitterOptions>>();
            testOptions.SetupGet(x => x.Value).Returns(_twitterOptions.Object);

            var adapter = new TwitterAdapter(testOptions.Object);

            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupAllProperties();
            httpResponse.Object.Body = new MemoryStream();

            var request = new Mock<HttpRequest>();
            request.SetupAllProperties();
            request.Object.Method = HttpMethods.Get;
            request.Setup(req => req.Query.TryGetValue("crc_token", out _values)).Returns(true);

            var context = new Mock<HttpContext>();
            context.SetupGet(x => x.Response).Returns(httpResponse.Object);
            context.SetupGet(x => x.Request).Returns(request.Object);

            var middleware = new WebhookMiddleware(testOptions.Object, _testLogger.Object, _testBot.Object, adapter);

            await middleware.InvokeAsync(context.Object, _testDelegate.Object);

            Assert.AreEqual(httpResponse.Object.StatusCode, (int)HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task InvokeAsyncInvalidShouldLogError()
        {
            var testOptions = new Mock<IOptions<TwitterOptions>>();
            testOptions.SetupGet(x => x.Value).Returns(_twitterOptions.Object);

            var adapter = new TwitterAdapter(testOptions.Object);

            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupAllProperties();
            httpResponse.Object.Body = new MemoryStream();

            var request = new Mock<HttpRequest>();
            request.SetupAllProperties();
            request.Object.Method = HttpMethods.Get;
            request.Setup(req => req.Query.TryGetValue("crc_token", out _values)).Returns(false);

            var context = new Mock<HttpContext>();
            context.SetupGet(x => x.Response).Returns(httpResponse.Object);
            context.SetupGet(x => x.Request).Returns(request.Object);

            var middleware = new WebhookMiddleware(testOptions.Object, _testLogger.Object, _testBot.Object, adapter);

            await middleware.InvokeAsync(context.Object, _testDelegate.Object);

            _testLogger.Verify(x => x.Log(LogLevel.Error, 0, It.IsAny<FormattedLogValues>(), null, It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
    }
}
