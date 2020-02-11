using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bot.Builder.Community.Adapters.Twitter.Tests
{
    [TestClass]
    public class WebhookInterceptorTests
    {
        private readonly string consumerSecret = "test";

        [TestMethod]
        public async Task InterceptIncomingRequestWithEmptyConsumerSecretShouldFail()
        {
            var interceptor = new WebhookInterceptor("");
            var request = new Mock<HttpRequest>();
            var action = new Mock<Action<DirectMessageEvent>>();

            await Assert.ThrowsExceptionAsync<TwitterException>(async () =>
            {
                await interceptor.InterceptIncomingRequest(request.Object, action.Object);
            });
        }

        [TestMethod]
        public async Task InterceptIncomingRequestWithEmptyRequestShouldReturnUnhandled()
        {
            var interceptor = new WebhookInterceptor(consumerSecret);
            var request = new Mock<HttpRequest>();
            var action = new Mock<Action<DirectMessageEvent>>();
            var response = await interceptor.InterceptIncomingRequest(request.Object, action.Object);

            Assert.IsFalse(response.IsHandled);
        }

        [TestMethod]
        public async Task InterceptIncomingRequestGetAndEmptyCrcTokenShouldReturnUnhandled()
        {
            var interceptor = new WebhookInterceptor(consumerSecret);
            var action = new Mock<Action<DirectMessageEvent>>();
            var request = new Mock<HttpRequest>();
            request.SetupAllProperties();
            request.Object.Method = HttpMethods.Get;
            request.Object.Query = new Mock<IQueryCollection>().Object;
            
            var response = await interceptor.InterceptIncomingRequest(request.Object, action.Object);

            Assert.IsFalse(response.IsHandled);
        }

        [TestMethod]
        public async Task InterceptIncomingRequestGetAndCrcTokenShouldReturnHandled()
        {
            var interceptor = new WebhookInterceptor(consumerSecret);
            var action = new Mock<Action<DirectMessageEvent>>();
            var request = new Mock<HttpRequest>();
            var values = new StringValues("test_code");
            request.SetupAllProperties();
            request.Object.Method = HttpMethods.Get;
            request.Setup(req => req.Query.TryGetValue("crc_token", out values)).Returns(true);

            var response = await interceptor.InterceptIncomingRequest(request.Object, action.Object);

            Assert.IsTrue(response.IsHandled);
        }

        [TestMethod]
        public async Task InterceptIncomingRequestPostWithEmptyHeadersShouldReturnUnhandled()
        {
            var interceptor = new WebhookInterceptor(consumerSecret);
            var request = new Mock<HttpRequest>();
            request.SetupAllProperties();
            request.Object.Method = HttpMethods.Post;

            StringValues values;
            request.Setup(req => req.Headers.TryGetValue("x-twitter-webhooks-signature", out values)).Returns(false);
            var action = new Mock<Action<DirectMessageEvent>>();
            var response = await interceptor.InterceptIncomingRequest(request.Object, action.Object);

            Assert.IsFalse(response.IsHandled);
        }

        [TestMethod]
        public async Task InterceptIncomingRequestPostWithInvalidSignatureShouldFail()
        {
            var interceptor = new WebhookInterceptor(consumerSecret);
            var action = new Mock<Action<DirectMessageEvent>>();
            var values = new StringValues("test_values");
            var request = new Mock<HttpRequest>();
            request.SetupAllProperties();
            request.Object.Method = HttpMethods.Post;
            request.Object.Body = new MemoryStream(Encoding.UTF8.GetBytes("test_body"));
            request.Setup(req => req.Headers.TryGetValue("x-twitter-webhooks-signature", out values)).Returns(true);
            
            await Assert.ThrowsExceptionAsync<TwitterException>(
                async () =>
            {
                await interceptor.InterceptIncomingRequest(request.Object, action.Object);
            }, "Invalid signature");
        }

        [TestMethod]
        public async Task InterceptIncomingRequestPostWithValidSignatureShouldReturnHandled()
        {
            var body = "test_body";
            var interceptor = new WebhookInterceptor(consumerSecret);
            var request = new Mock<HttpRequest>();
            var action = new Mock<Action<DirectMessageEvent>>();
            var hashKeyArray = Encoding.UTF8.GetBytes(consumerSecret);
            var hmacSha256Alg = new HMACSHA256(hashKeyArray);
            var computedHash = hmacSha256Alg.ComputeHash(Encoding.UTF8.GetBytes(body));
            var localHashedSignature = $"sha256={Convert.ToBase64String(computedHash)}";
            var values = new StringValues(localHashedSignature);

            request.SetupAllProperties();
            request.Object.Method = HttpMethods.Post;
            request.Object.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            request.Setup(req => req.Headers.TryGetValue("x-twitter-webhooks-signature", out values)).Returns(true);

            var response = await interceptor.InterceptIncomingRequest(request.Object, action.Object);

            Assert.IsTrue(response.IsHandled);
        }
    }
}
