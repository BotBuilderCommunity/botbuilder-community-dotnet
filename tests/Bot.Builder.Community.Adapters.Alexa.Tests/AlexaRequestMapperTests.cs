using Bot.Builder.Community.Adapters.Alexa.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Bot.Builder.Community.Adapters.Alexa.Tests
{
    public class AlexaRequestMapperTests
    {
        [Fact]
        public void ConstructorWithNoArgumentsShouldSucceed()
        {
            Assert.NotNull(new AlexaRequestMapper());
        }

        [Fact]
        public void ConstructorWithOptionsOnlyShouldSucceed()
        {
            Assert.NotNull(new AlexaRequestMapper(new Mock<AlexaRequestMapperOptions>().Object));
        }

        [Fact]
        public void ConstructorWithLoggerOnlyShouldSucceed()
        {
            Assert.NotNull(new AlexaRequestMapper(logger: new LoggerFactory().CreateLogger("test")));
        }

        [Fact]
        public void MergeActivitiesReturnsNullWithNoActivities()
        {
            var alexaAdapter = new AlexaRequestMapper();
            Assert.Null(alexaAdapter.MergeActivities(new List<Activity>()));
        }

        [Fact]
        public void MergeActivitiesReturnIdenticalSingleActivityNoSsml()
        {
            var alexaAdapter = new AlexaRequestMapper();
            var inputActivity = MessageFactory.Text("This is the single activity", "<speak>This is<break strength=\"strong\"/>the SSML</speak>");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { inputActivity });

            Assert.Equal(inputActivity, processActivityResult);
        }

        [Fact]
        public void MergeActivitiesReturnsIdenticalSingleActivityWithSpeakSsmlTag()
        {
            var alexaAdapter = new AlexaRequestMapper();
            var inputActivity = MessageFactory.Text("This is the single activity", "<speak>This is<break strength=\"strong\"/>the SSML</speak>");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { inputActivity });

            Assert.Equal(inputActivity, processActivityResult);
        }

        [Fact]
        public void MergeActivitiesReturnsSingleActivityAddingSpeakSsmlTag()
        {
            var alexaAdapter = new AlexaRequestMapper();
            var inputActivity = MessageFactory.Text("This is the single activity", "This is<break strength=\"strong\"/>the SSML");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { inputActivity });

            Assert.Equal(inputActivity, processActivityResult);
        }

        [Fact]
        public void MergeActivitiesReturnsCorrectlyJoinedText()
        {
            var alexaAdapter = new AlexaRequestMapper();

            var firstActivity = MessageFactory.Text("This is the first activity.");
            var secondActivity = MessageFactory.Text("This is the second activity");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            Assert.Equal("This is the first activity. This is the second activity", processActivityResult.Text);
        }

        [Fact]
        public void MergeActivitiesReturnsCorrectlyJoinedSpeakWithSsml()
        {
            var alexaAdapter = new AlexaRequestMapper();

            // Note: The input activities deliberately have an activity where the speak tag
            // is included and one activity where it is not, to ensure the stripping / wrapping
            // of the speak tag is handled correctly.
            var firstActivity = MessageFactory.Text("This is the first activity.", "This is<break strength=\"strong\"/>the first activity SSML");
            var secondActivity = MessageFactory.Text("This is the second activity.", "<speak>This is the second activity SSML</speak>");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            Assert.Equal("<speak>This is<break strength=\"strong\"/>the first activity SSML<break strength=\"strong\"/>This is the second activity SSML</speak>", 
                processActivityResult.Speak);
        }
    }
}
