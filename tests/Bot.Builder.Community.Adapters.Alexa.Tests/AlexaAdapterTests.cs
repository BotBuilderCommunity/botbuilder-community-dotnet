using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bot.Builder.Community.Adapters.Alexa.Tests
{
    public class AlexaTests
    {
        [Fact]
        public void ConstructorWithNoArgumentsShouldSucceed()
        {
            Assert.NotNull(new AlexaAdapter());
        }

        [Fact]
        public void ConstructorWithOptionsOnlyShouldSucceed()
        {
            Assert.NotNull(new AlexaAdapter(new Mock<AlexaAdapterOptions>().Object));
        }

        [Fact]
        public async void ContinueConversationAsyncShouldSucceed()
        {
            var callbackInvoked = false;

            var alexaAdapter = new AlexaAdapter(new Mock<AlexaAdapterOptions>().Object);
            var conversationReference = new ConversationReference();
            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            }

            await alexaAdapter.ContinueConversationAsync(conversationReference, BotsLogic, default);
            Assert.True(callbackInvoked);
        }

        [Fact]
        public async void ContinueConversationAsyncShouldFailWithNullConversationReference()
        {
            var alexaAdapter = new AlexaAdapter(new Mock<AlexaAdapterOptions>().Object);

            static Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await alexaAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }
    }
}
