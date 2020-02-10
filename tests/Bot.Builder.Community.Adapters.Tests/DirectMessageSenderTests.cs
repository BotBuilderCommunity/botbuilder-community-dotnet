using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Bot.Builder.Community.Adapters.Twitter.Tests
{
    [TestClass]
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
                await sender.Send("test", "");
            }, "You can't send an empty message.");
        }

        [TestMethod]
        public async Task SendWithEmptyLongMessageShouldFail()
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
