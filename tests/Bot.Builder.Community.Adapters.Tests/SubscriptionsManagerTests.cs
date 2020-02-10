using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bot.Builder.Community.Adapters.Twitter.Tests
{
    [TestClass]
    public class SubscriptionsManagerTests
    {

        private readonly Mock<IOptions<TwitterOptions>> _testOptions = new Mock<IOptions<TwitterOptions>>();

        [TestMethod]
        public async Task SubscribeWithEmptyEnvironmentNameShouldFail()
        {
            var subscriptionManager = new SubscriptionsManager(_testOptions.Object.Value);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await subscriptionManager.Subscribe("");
                });
        }

        [TestMethod]
        public async Task CheckSubscriptionWithEmptyEnvironmentNameShouldFail()
        {
            var subscriptionManager = new SubscriptionsManager(_testOptions.Object.Value);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await subscriptionManager.CheckSubscription("");
            });
        }

        [TestMethod]
        public async Task UnsubscribeWithEmptyEnvironmentNameShouldFail()
        {
            var subscriptionManager = new SubscriptionsManager(_testOptions.Object.Value);

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () =>
            {
                await subscriptionManager.Unsubscribe("");
            });
        }
    }
}
