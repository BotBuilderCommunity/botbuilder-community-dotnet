using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Alexa.Core;
using Bot.Builder.Community.Adapters.Alexa.Tests.Utility;
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

        [Fact]
        public void PlainTextMessageActivityConverted()
        {
            var skillRequest = SkillRequestUtility.CreateIntentRequest();
            var mapper = new AlexaRequestMapper();

            var activity = Activity.CreateMessageActivity() as Activity;
            activity.Text = "Hello world";
            activity.TextFormat = TextFormatTypes.Plain;

            var skillResponse = mapper.ActivityToResponse(activity, skillRequest);
            VerifyPlainTextResponse(skillResponse, activity.Text);
        }

        [Fact]
        public void NonMessageActivityConverted()
        {
            var skillRequest = SkillRequestUtility.CreateIntentRequest();
            var mapper = new AlexaRequestMapper();

            var activity = Activity.CreateTraceActivity("This is a trace") as Activity;

            var skillResponse = mapper.ActivityToResponse(activity, skillRequest);
            VerifyPlainTextResponse(skillResponse, string.Empty);
        }

        [Fact]
        public void SimpleIntentRequest()
        {
            var skillRequest = SkillRequestUtility.CreateIntentRequest();
            var mapperOptions = new AlexaRequestMapperOptions { ServiceUrl = "service url" };
            var mapper = new AlexaRequestMapper(mapperOptions);

            ((IntentRequest)skillRequest.Request).Intent.Slots[mapperOptions.DefaultIntentSlotName].Value = "hello world";

            var convertedActivity = mapper.RequestToActivity(skillRequest);

            VerifyIntentRequest(skillRequest, convertedActivity, mapperOptions);
        }

        private static void VerifyIntentRequest(SkillRequest skillRequest, IActivity activity, AlexaRequestMapperOptions mapperOptions)
        {
            VerifyRequest(skillRequest, activity, mapperOptions);

            var messageActivity = activity.AsMessageActivity();
            Assert.NotNull(messageActivity);

            Assert.Equal(DeliveryModes.ExpectReplies, messageActivity.DeliveryMode);
            Assert.Equal(skillRequest.Request.Locale, messageActivity.Locale);
            Assert.Equal(((IntentRequest)skillRequest.Request).Intent.Slots[mapperOptions.DefaultIntentSlotName].Value, messageActivity.Text);
        }

        private static void VerifyRequest(SkillRequest skillRequest, IActivity activity, AlexaRequestMapperOptions mapperOptions)
        {
            Assert.NotNull(activity);
            Assert.Equal(skillRequest, activity.ChannelData);
            Assert.Equal(mapperOptions.ChannelId, activity.ChannelId);

            Assert.NotNull(activity.Conversation);
            Assert.Equal("conversation", activity.Conversation.ConversationType);
            Assert.Equal(skillRequest.Session.SessionId, activity.Conversation.Id);
            Assert.Equal(false, activity.Conversation.IsGroup);

            Assert.NotNull(activity.From);
            Assert.Equal(skillRequest.Context.System.Person?.PersonId ?? skillRequest.Context.System.User.UserId, activity.From.Id);
            Assert.Equal(skillRequest.Request.RequestId, activity.Id);


            Assert.NotNull(activity.Recipient);
            Assert.Equal(skillRequest.Context.System.Application.ApplicationId, activity.Recipient.Id);

            Assert.Equal(mapperOptions.ServiceUrl, activity.ServiceUrl);
            Assert.Equal(skillRequest.Request.Timestamp, activity.Timestamp);
            Assert.Equal(ActivityTypes.Message, activity.Type);
        }

        private static void VerifyPlainTextResponse(SkillResponse skillResponse, string text)
        {
            Assert.Equal("1.0", skillResponse.Version);
            Assert.Null(skillResponse.Response.Card);
            Assert.NotNull(skillResponse.Response.Directives);
            Assert.Equal(0, skillResponse.Response.Directives.Count);
            Assert.NotNull(skillResponse.Response.OutputSpeech);
            Assert.Null(skillResponse.Response.OutputSpeech.PlayBehavior);
            Assert.Equal("PlainText", skillResponse.Response.OutputSpeech.Type);
            var plainTextOutputSpeech = skillResponse.Response.OutputSpeech as PlainTextOutputSpeech;
            Assert.NotNull(plainTextOutputSpeech);
            Assert.Equal(text, plainTextOutputSpeech.Text);
            Assert.Null(plainTextOutputSpeech.PlayBehavior);
            Assert.Null(skillResponse.Response.Reprompt);
            Assert.Equal(true as bool?, skillResponse.Response.ShouldEndSession);
            Assert.Null(skillResponse.SessionAttributes);
        }
    }
}
