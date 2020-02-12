using System.Net.Http;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Authentication;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Castle.Core.Internal;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bot.Builder.Community.Adapters.Twitter.Tests.Webhooks.Authentication
{
    [TestClass]
    [TestCategory("Twitter")]
    public class AuthHeaderBuilderTests
    {
        private readonly Mock<IOptions<TwitterOptions>> _testOptions = new Mock<IOptions<TwitterOptions>>();

        [TestMethod]
        public void BuildWithNullUrlShouldFail()
        {
            Assert.ThrowsException<TwitterException>(
                () =>
            {
                AuthHeaderBuilder.Build(_testOptions.Object.Value, HttpMethod.Post, null);
            }, "Invalid Resource Url format.");
        }

        [TestMethod]
        public void BuildWithInvalidOptionsShouldFail()
        {
            Assert.ThrowsException<TwitterException>(
                () =>
                {
                    AuthHeaderBuilder.Build(_testOptions.Object.Value, HttpMethod.Post, "test_url");
                }, "Invalid Twitter options.");
        }

        [TestMethod]
        public void BuildWithValidOptionsAndParametersShouldReturnHeader()
        {
            var testOptions = new Mock<TwitterOptions>()
            {
                Object =
                {
                    ConsumerKey = "ConsumerKey",
                    ConsumerSecret = "ConsumerSecret",
                    AccessToken = "AccessToken",
                    AccessSecret = "AccessSecret",
                    WebhookUri = "WebhookUri",
                    Environment = "Environment"
                }
            };
            var header = AuthHeaderBuilder.Build(testOptions.Object, HttpMethod.Get, "http://test_url.com?param1=val1&param2=val2");
            Assert.IsFalse(header.IsNullOrEmpty());
        }

        [TestMethod]
        public void BuildWithValidOptionsAndEmptyParametersShouldReturnHeader()
        {
            var testOptions = new Mock<TwitterOptions>()
            {
                Object =
                {
                    ConsumerKey = "ConsumerKey",
                    ConsumerSecret = "ConsumerSecret",
                    AccessToken = "AccessToken",
                    AccessSecret = "AccessSecret",
                    WebhookUri = "WebhookUri",
                    Environment = "Environment"
                }
            };
            var header = AuthHeaderBuilder.Build(testOptions.Object, HttpMethod.Get, "test_url");
            Assert.IsFalse(header.IsNullOrEmpty());
        }
    }
}
