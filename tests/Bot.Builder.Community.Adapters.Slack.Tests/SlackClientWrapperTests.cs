using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Slack.Tests
{
    public class SlackClientWrapperTests
    {
        private readonly SlackClientWrapperOptions _testOptions = new SlackClientWrapperOptions("VerificationToken", "ClientSigningSecret", "BotToken");

        [Fact]
        public void VerifySignatureShouldReturnFalseWithNullParameters()
        {
            var slackApi = new SlackClientWrapper(_testOptions);

            Assert.False(slackApi.VerifySignature(null, null));
        }

        [Fact]
        public void VerifySignatureShouldReturnTrue()
        {
            var slackApi = new SlackClientWrapper(_testOptions);

            var body = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/MessageBody.json");

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers["X-Slack-Request-Timestamp"]).Returns("0001-01-01T00:00:00+00:00");
            httpRequest.SetupGet(req => req.Headers["X-Slack-Signature"]).Returns("V0=CD482F6E327EB7B5882890614EB87CB20831C5A527CC4D419A3B3BCD4BC8761C");

            Assert.True(slackApi.VerifySignature(httpRequest.Object, body));
        }
    }
}
