using System;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bot.Builder.Community.Adapters.Twitter.Tests
{
    [TestClass]
    [TestCategory("Twitter")]
    public class DirectMessageSenderTests
    {
        private readonly Mock<IOptions<TwitterOptions>> _testOptions = new Mock<IOptions<TwitterOptions>>();

        [TestMethod]
        public async Task SendWithEmptyMessageShouldFail()
        {
            var sender = new DirectMessageSender(_testOptions.Object.Value);

            await Assert.ThrowsExceptionAsync<TwitterException>(
                async () =>
            {
                await sender.Send("test", string.Empty);
            }, "You can't send an empty message.");
        }

        [TestMethod]
        public async Task SendWithLongMessageShouldFail()
        {
            var sender = new DirectMessageSender(_testOptions.Object.Value);

            await Assert.ThrowsExceptionAsync<TwitterException>(
                async () =>
                {
                    await sender.Send("test", new String('a', 141));
                }, "You can't send more than 140 char using this end point, use SendAsync instead.");
        }
    }
}
