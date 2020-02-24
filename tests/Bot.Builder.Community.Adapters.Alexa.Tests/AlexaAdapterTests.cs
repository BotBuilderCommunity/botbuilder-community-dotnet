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
        public void ConstructorShouldSucceedWithNullOptions()
        {
            Assert.NotNull(new AlexaAdapter());
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

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await alexaAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }

        [Fact]
        public void ProcessOutgoingActivitiesReturnsNullWithNoActivities()
        {
            var alexaAdapter = new AlexaAdapter(new Mock<AlexaAdapterOptions>().Object);
            Assert.Null(alexaAdapter.ProcessOutgoingActivities(new List<Activity>()));
        }

        [Fact]
        public void ProcessOutgoingActivitiesReturnIdenticalSingleActivityNoSsml()
        {
            var alexaAdapter = new AlexaAdapter(new Mock<AlexaAdapterOptions>().Object);
            var inputActivity = MessageFactory.Text("This is the single activity", "<speak>This is<break strength=\"strong\"/>the SSML</speak>");

            var processActivityResult = alexaAdapter.ProcessOutgoingActivities(new List<Activity>() { inputActivity });

            Assert.Equal(inputActivity, processActivityResult);
        }

        [Fact]
        public void ProcessOutgoingActivitiesReturnsIdenticalSingleActivityWithSpeakSsmlTag()
        {
            var alexaAdapter = new AlexaAdapter(new Mock<AlexaAdapterOptions>().Object);
            var inputActivity = MessageFactory.Text("This is the single activity", "<speak>This is<break strength=\"strong\"/>the SSML</speak>");

            var processActivityResult = alexaAdapter.ProcessOutgoingActivities(new List<Activity>() { inputActivity });

            Assert.Equal(inputActivity, processActivityResult);
        }

        [Fact]
        public void ProcessOutgoingActivitiesReturnsSingleActivityAddingSpeakSsmlTag()
        {
            var alexaAdapter = new AlexaAdapter(new Mock<AlexaAdapterOptions>().Object);
            var inputActivity = MessageFactory.Text("This is the single activity", "This is<break strength=\"strong\"/>the SSML");

            var processActivityResult = alexaAdapter.ProcessOutgoingActivities(new List<Activity>() { inputActivity });

            Assert.Equal(inputActivity, processActivityResult);
        }

        [Fact]
        public void ProcessOutgoingActivitiesReturnsCorrectlyJoinedText()
        {
            var alexaAdapter = new AlexaAdapter(new Mock<AlexaAdapterOptions>().Object);
            
            var firstActivity = MessageFactory.Text("This is the first activity.");
            var secondActivity = MessageFactory.Text("This is the second activity");

            var processActivityResult = alexaAdapter.ProcessOutgoingActivities(new List<Activity>() { firstActivity, secondActivity });

            Assert.Equal("This is the first activity. This is the second activity", processActivityResult.Text);
        }

        [Fact]
        public void ProcessOutgoingActivitiesReturnsCorrectlyJoinedSpeakWithSsml()
        {
            var alexaAdapter = new AlexaAdapter(new Mock<AlexaAdapterOptions>().Object);
            
            // Note: The input activities deliberately have an activity where the speak tag
            // is included and one activity where it is not, to ensure the stripping / wrapping
            // of the speak tag is handled correctly.
            var firstActivity = MessageFactory.Text("This is the first activity.", "This is<break strength=\"strong\"/>the first activity SSML");
            var secondActivity = MessageFactory.Text("This is the second activity.", "<speak>This is the second activity SSML</speak>");

            var processActivityResult = alexaAdapter.ProcessOutgoingActivities(new List<Activity>() { firstActivity, secondActivity });

            Assert.Equal("<speak>This is<break strength=\"strong\"/>the first activity SSML<break strength=\"strong\"/>This is the second activity SSML</speak>", 
                processActivityResult.Speak);
        }
    }
}
