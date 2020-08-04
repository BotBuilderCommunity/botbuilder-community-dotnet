using System;
using System.Collections.Generic;
using System.Text;
using Alexa.NET.ConnectionTasks.Inputs;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Directive.Templates;
using Alexa.NET.Response.Directive.Templates.Types;
using Bot.Builder.Community.Adapters.Alexa.Core;
using Bot.Builder.Community.Adapters.Alexa.Core.Attachments;
using Bot.Builder.Community.Adapters.Alexa.Tests.Helpers;
using FluentAssertions.Common;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
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
        public void MergeActivitiesReturnsNullWithNullActivities()
        {
            var alexaAdapter = new AlexaRequestMapper();
            Assert.Null(alexaAdapter.MergeActivities(null));
        }

        [Fact]
        public void MergeActivitiesReturnsNullWithNoActivities()
        {
            var alexaAdapter = new AlexaRequestMapper();
            Assert.Null(alexaAdapter.MergeActivities(new List<Activity>()));
        }

        [Fact]
        public void IntentRequestMappedReturnsLocale()
        {
            // arrange
            var skillRequest = new SkillRequest
            {
                Request = new IntentRequest()
                {
                    Locale = "en-US",
                    Intent = new Intent()
                    {
                        Slots = new Dictionary<string, Slot>()
                        {
                            { "phrase", new Slot() }
                        }
                    }
                },
                Context = new Context
                {
                    System = new AlexaSystem() { Application = new Application(), User = new User() }
                },
                Session = new Session()
                {
                    User = new User()
                }
            };


            // act
            var alexaMapper = new AlexaRequestMapper();
            var result = alexaMapper.RequestToActivity(skillRequest);

            // assert
            Assert.Equal("en-US", result.Locale);
        }

        [Fact]
        public void LaunchRequestMappedReturnsNull()
        {
            // arrange
            var skillRequest = new SkillRequest
            {
                Request = new LaunchRequest() {},
                Context = new Context
                {
                    System = new AlexaSystem() { Application = new Application(), User = new User() }
                },
                Session = new Session()
                {
                    User = new User()
                }
            };


            // act
            var alexaMapper = new AlexaRequestMapper();
            var result = alexaMapper.RequestToActivity(skillRequest);

            // assert
            Assert.Null(result.Locale);
        }

        [Fact]
        public void IntentRequestMappedReturnsNull()
        {
            // arrange
            var skillRequest = new SkillRequest
            {
                Request = new IntentRequest() { Intent = new Intent() 
                    { 
                        Slots = new Dictionary<string, Slot>()
                        {
                            { "phrase", new Slot() }
                        }
                    }
                },
                Context = new Context
                {
                    System = new AlexaSystem() { Application = new Application(), User = new User() }
                },
                Session = new Session()
                {
                    User = new User()
                }
            };


            // act
            var alexaMapper = new AlexaRequestMapper();
            var result = alexaMapper.RequestToActivity(skillRequest);

            // assert
            Assert.Null(result.Locale);
        }

        [Fact]
        public void LaunchRequestMappedReturnsLocale()
        {
            // arrange
            var skillRequest = new SkillRequest
            {
                Request = new LaunchRequest() {Locale = "en-US"},
                Context = new Context
                {
                    System = new AlexaSystem() {Application = new Application(), User = new User()}
                },
                Session = new Session()
                {
                    User = new User()
                }
            };


            // act
            var alexaMapper = new AlexaRequestMapper();
            var result = alexaMapper.RequestToActivity(skillRequest);

            // assert
            Assert.Equal("en-US", result.Locale);
        }

        [Fact]
        public void MergeActivitiesReturnIdenticalSingleActivityNoSsml()
        {
            const string text = "This is the single activity";
            const string ssml = null;

            var alexaAdapter = new AlexaRequestMapper();
            var inputActivity = MessageFactory.Text(text, ssml);

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { inputActivity });

            Assert.Equal(text, processActivityResult.MergedActivity.Text);
            Assert.Equal(ssml, processActivityResult.MergedActivity.Speak);
        }

        [Fact]
        public void MergeActivitiesReturnsIdenticalSingleActivityWithSpeakSsmlTag()
        {
            const string text = "This is the single activity";
            const string ssml = "<speak>This is<break strength=\"strong\"/>the SSML</speak>";

            var alexaAdapter = new AlexaRequestMapper();
            var inputActivity = MessageFactory.Text(text, ssml);

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { inputActivity });

            Assert.Equal(text, processActivityResult.MergedActivity.Text);
            // When removing the speak tag the serializer adds the missing space at the end of the xml element. This doesn't matter for rendering in Alexa so it is fine.
            Assert.Equal(ssml.Replace("/>", " />"), processActivityResult.MergedActivity.Speak);
        }

        [Fact]
        public void MergeActivitiesReturnsSingleActivityAddingSpeakSsmlTag()
        {
            const string text = "This is the single activity";
            const string ssml = "This is<break strength=\"strong\"/>the SSML";

            var alexaAdapter = new AlexaRequestMapper();
            var inputActivity = MessageFactory.Text(text, ssml);

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { inputActivity });

            Assert.Equal(text, processActivityResult.MergedActivity.Text);
            Assert.Equal($"<speak>{ssml}</speak>", processActivityResult.MergedActivity.Speak);
        }

        [Fact]
        public void MergeActivitiesReturnsCorrectlyJoinedText()
        {
            var alexaAdapter = new AlexaRequestMapper();

            var firstActivity = MessageFactory.Text("This is the first activity.");
            var secondActivity = MessageFactory.Text("This is the second activity");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            Assert.Equal("This is the first activity. This is the second activity", processActivityResult.MergedActivity.Text);
        }

        [Fact]
        public void MergeActivitiesAttachmentAndPlainText()
        {
            var alexaAdapter = new AlexaRequestMapper();

            var attachment = new Attachment
            {
                Name = "Attachment1.jpg",
                ContentType = "image/jpeg",
                Content = "https://somefantasticurl/",
            };
            var firstActivity = MessageFactory.Text(JsonConvert.SerializeObject(attachment, HttpHelper.BotMessageSerializerSettings));
            var secondActivity = (Activity) MessageFactory.Attachment(attachment);

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity> { firstActivity, secondActivity });

            Assert.Equal("{   \"contentType\": \"image/jpeg\",   \"content\": \"https://somefantasticurl/\",   \"name\": \"Attachment1.jpg\" }", processActivityResult.MergedActivity.Text);
        }

        [Fact]
        public void MergeActivitiesMergesAttachments()
        {
            var alexaAdapter = new AlexaRequestMapper();

            var firstActivity = MessageFactory.Text("This is the first activity.");
            var secondActivity = MessageFactory.Text("This is the second activity");

            firstActivity.Attachments.Add(new SimpleCard { Title = "Simple card title", Content = "Test content"}.ToAttachment());
            secondActivity.Attachments = null;

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            Assert.Equal("This is the first activity. This is the second activity", processActivityResult.MergedActivity.Text);
            Assert.NotNull(processActivityResult.MergedActivity.Attachments);
            Assert.Equal(1, processActivityResult.MergedActivity.Attachments.Count);
            Assert.Equal(AlexaAttachmentContentTypes.Card, processActivityResult.MergedActivity.Attachments[0].ContentType);
        }

        [Fact]
        public void MergeActivitiesTextWithPeriodAndEmptyText()
        {
            var alexaAdapter = new AlexaRequestMapper();
            var firstActivity = MessageFactory.Text("This is the first activity.");
            var secondActivity = MessageFactory.Text("");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            // We want to preserve the period here even though it is merged with empty text.
            Assert.Equal("This is the first activity.", processActivityResult.MergedActivity.Text);
        }

        [Fact]
        public void MergeActivitiesTextWithNoPeriodAndEmptyText()
        {
            var alexaAdapter = new AlexaRequestMapper();
            var firstActivity = MessageFactory.Text("This is the first activity");
            var secondActivity = MessageFactory.Text("");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            // We want to preserve the missing period here even though it is merged with empty text.
            Assert.Equal("This is the first activity", processActivityResult.MergedActivity.Text);
        }

        [Fact]
        public void MergeActivitiesIgnoresNonMessageActivities()
        {
            var alexaAdapter = new AlexaRequestMapper();

            var firstActivity = MessageFactory.Text("This is the first activity.");
            var traceActivity = Activity.CreateTraceActivity("This is a trace") as Activity;
            traceActivity.Text = "This text won't show up";
            var secondActivity = MessageFactory.Text("This is the second activity");
            var typingActivity = Activity.CreateTypingActivity() as Activity;

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { firstActivity, traceActivity, secondActivity, typingActivity });

            Assert.Equal("This is the first activity. This is the second activity", processActivityResult.MergedActivity.Text);
        }

        [Fact]
        public void MergeActivitiesReturnsCorrectlyJoinedSpeakWithSsml()
        {
            var alexaAdapter = new AlexaRequestMapper();

            // Note: The input activities deliberately have an activity where the speak tag
            // is included and one activity where it is not, to ensure the stripping / wrapping
            // of the speak tag is handled correctly.
            var firstActivity = MessageFactory.Text("This is the first activity.", "This is<break strength=\"strong\"/>the first activity SSML");
            var secondActivity = MessageFactory.Text("This is the second activity", "<speak>This is the second activity SSML</speak>");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            Assert.Equal("<speak>This is<break strength=\"strong\"/>the first activity SSML<break strength=\"strong\"/>This is the second activity SSML</speak>", processActivityResult.MergedActivity.Speak);
            Assert.Equal("This is the first activity. This is the second activity", processActivityResult.MergedActivity.Text);
            Assert.False(processActivityResult.EndOfConversationFlagged);
        }

        [Fact]
        public void MergeActivitiesWithEndOfConversationOnly()
        {
            var alexaAdapter = new AlexaRequestMapper();

            var activity = (Activity) Activity.CreateEndOfConversationActivity();

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { activity });

            // Empty merged activities.
            Assert.NotNull(processActivityResult);
            Assert.Null(processActivityResult.MergedActivity);
            Assert.True(processActivityResult.EndOfConversationFlagged);
        }

        [Fact]
        public void MergeActivitiesJoinedTextWithEndOfConversation()
        {
            var alexaAdapter = new AlexaRequestMapper();

            var activites = new List<Activity>()
            {
                MessageFactory.Text("This is the first activity."),
                (Activity)Activity.CreateEndOfConversationActivity(),
                MessageFactory.Text("This is the second activity")
            };

            var processActivityResult = alexaAdapter.MergeActivities(activites);

            Assert.Equal("This is the first activity. This is the second activity", processActivityResult.MergedActivity.Text);
            Assert.True(processActivityResult.EndOfConversationFlagged);
        }

        [Fact]
        public void MergeActivitiesCorrectlyConvertsMarkdownToPlainText()
        {
            var alexaAdapter = new AlexaRequestMapper();

            // Note: The input activities deliberately have an activity where the speak tag
            // is included and one activity where it is not, to ensure the stripping / wrapping
            // of the speak tag is handled correctly.

            var message = new StringBuilder();
            message.AppendLine("**This** ~~is~~ ***the*** first activity.");
            message.AppendLine("# Heading 1");
            message.AppendLine("This is another paragraph.");
            message.AppendLine("- Item 1");
            message.AppendLine("- Item 2");
            message.AppendLine("- Item 3");
            message.AppendLine("");
            message.AppendLine("## Heading 2");
            message.AppendLine("1. Item 1");
            message.AppendLine("2. Item 2");
            message.AppendLine("3. Item 3");
            message.AppendLine("");
            message.AppendLine("More info [visit our web site](www.microsoft.com)");

            var firstActivity = MessageFactory.Text(message.ToString(), "This is<break strength=\"strong\"/>the first activity SSML");
            var secondActivity = MessageFactory.Text("This is the second activity.", "<speak>This is the second activity SSML</speak>");

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { firstActivity, secondActivity });

            Assert.Equal("This is the first activity. Heading 1. This is another paragraph. Item 1, Item 2, Item 3. Heading 2. 1. Item 1, 2. Item 2, 3. Item 3. More info visit our web site www.microsoft.com. This is the second activity.",
                processActivityResult.MergedActivity.Text);

            Assert.Equal("<speak>This is<break strength=\"strong\"/>the first activity SSML<break strength=\"strong\"/>This is the second activity SSML</speak>",
                processActivityResult.MergedActivity.Speak);
        }

        [Fact]
        public void PlainTextMessageActivityConverted()
        {
            var skillRequest = SkillRequestHelper.CreateIntentRequest();
            var mapper = new AlexaRequestMapper();

            var activity = Activity.CreateMessageActivity() as Activity;
            activity.Text = "Hello world";
            activity.TextFormat = TextFormatTypes.Plain;

            var skillResponse = ExecuteActivityToResponse(mapper, new MergedActivityResult { MergedActivity = activity }, skillRequest);
            VerifyPlainTextResponse(skillResponse, activity.Text);
        }

        [Fact]
        public void AppostrophiesNotConvertedNonSsml()
        {
            const string text = "It's amazing <xml />";
            var alexaAdapter = new AlexaRequestMapper();

            var skillRequest = SkillRequestHelper.CreateIntentRequest();
            var mapper = new AlexaRequestMapper();

            var activity = Activity.CreateMessageActivity() as Activity;
            activity.Text = text;

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { activity });

            var skillResponse = ExecuteActivityToResponse(mapper, processActivityResult, skillRequest);
            VerifyPlainTextResponse(skillResponse, text);
        }

        [Fact]
        public void AppostrophiesNotConvertedSsml()
        {
            const string text = "It's amazing <break /> really";
            var alexaAdapter = new AlexaRequestMapper();

            var skillRequest = SkillRequestHelper.CreateIntentRequest();
            var mapper = new AlexaRequestMapper();

            var activity = Activity.CreateMessageActivity() as Activity;
            activity.Speak = text;

            var processActivityResult = alexaAdapter.MergeActivities(new List<Activity>() { activity });

            var skillResponse = ExecuteActivityToResponse(mapper, processActivityResult, skillRequest);
            var outputSpeech = skillResponse.Response.OutputSpeech as SsmlOutputSpeech;
            Assert.Equal($"<speak>{text}</speak>", outputSpeech.Ssml);
        }

        [Fact]
        public void NonMessageActivityConverted()
        {
            var skillRequest = SkillRequestHelper.CreateIntentRequest();
            var mapper = new AlexaRequestMapper();

            var activity = Activity.CreateTraceActivity("This is a trace") as Activity;

            var skillResponse = ExecuteActivityToResponse(mapper, new MergedActivityResult { MergedActivity = activity }, skillRequest);
            VerifyPlainTextResponse(skillResponse, string.Empty);
        }

        [Fact]
        public void SimpleIntentRequestConverted()
        {
            var skillRequest = SkillRequestHelper.CreateIntentRequest();
            var mapperOptions = new AlexaRequestMapperOptions { ServiceUrl = "service url" };
            var mapper = new AlexaRequestMapper(mapperOptions);

            ((IntentRequest)skillRequest.Request).Intent.Slots[mapperOptions.DefaultIntentSlotName].Value = "hello world";

            var convertedActivity = mapper.RequestToActivity(skillRequest);

            VerifyIntentRequest(skillRequest, convertedActivity, mapperOptions);
        }

        [Fact]
        public void MessageActivityWithAlexaCardDirectiveAttachmentsConverted()
        {
            var skillRequest = SkillRequestHelper.CreateIntentRequest();
            var mapper = new AlexaRequestMapper();

            var activity = Activity.CreateMessageActivity() as Activity;
            activity.Text = "Hello world";

            var hintDirective = new HintDirective("hint text");
            
            var displayDirective = new DisplayRenderTemplateDirective()
            {
                Template = new BodyTemplate1()
                {
                    BackgroundImage = new TemplateImage()
                    {
                        ContentDescription = "Test",
                        Sources = new List<ImageSource>()
                        {
                            new ImageSource()
                            {
                                Url = "https://via.placeholder.com/576.png/09f/fff",
                            }
                        }
                    },
                    Content = new TemplateContent()
                    {
                        Primary = new TemplateText() { Text = "Test", Type = "PlainText" }
                    },
                    Title = "Test title",
                }
            };

            var simpleCard = new SimpleCard()
            {
                Title = "This is a simple card",
                Content = "This is the simple card content"
            };

            activity.Attachments.Add(hintDirective.ToAttachment());
            activity.Attachments.Add(displayDirective.ToAttachment());
            activity.Attachments.Add(simpleCard.ToAttachment());

            var skillResponse = ExecuteActivityToResponse(mapper, new MergedActivityResult { MergedActivity = activity }, skillRequest);

            VerifyCardAttachmentAndDirectiveResponse(skillResponse, simpleCard, new List<IDirective>() { hintDirective, displayDirective });
        }

        [Fact]
        public void MessageActivityWithHeroCardConverted()
        {
            var skillRequest = SkillRequestHelper.CreateIntentRequest();
            var mapper = new AlexaRequestMapper();

            var heroCard = new HeroCard
            {
                Title = "Card title",
                Text = "Card text",
                Images = new List<Microsoft.Bot.Schema.CardImage>
                {
                    new Microsoft.Bot.Schema.CardImage() { Url = "https://url" }
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction { Title = "Places to buy", Type=ActionTypes.ImBack, Value="Places To Buy" },
                    new CardAction
                    {
                        Title = "I feel lucky",
                        Type=ActionTypes.OpenUrl,
                        Image = "https://image",
                        Value="https://value"
                    }
                }
            };

            var activity = Activity.CreateMessageActivity() as Activity;
            activity.Attachments.Add(new Attachment() { ContentType = HeroCard.ContentType, Content = heroCard});

            var skillResponse = ExecuteActivityToResponse(mapper, new MergedActivityResult { MergedActivity = activity }, skillRequest);

            Assert.NotNull(skillResponse.Response.Card);
            Assert.Equal(typeof(StandardCard), skillResponse.Response.Card.GetType());
            var card = skillResponse.Response.Card as StandardCard;
            Assert.Equal(heroCard.Text, card.Content);
            Assert.Equal(heroCard.Title, card.Title);
            Assert.Equal(heroCard.Images[0].Url, heroCard.Images[0].Url);
        }

        [Fact]
        public void MessageActivityWithSignInCard()
        {
            var skillRequest = SkillRequestHelper.CreateIntentRequest();
            var mapper = new AlexaRequestMapper();

            var signinCard = new SigninCard
            {
                Text = "sign in text",
                Buttons = new List<CardAction>()
                {
                    new CardAction
                    {
                        Title = "sign in",
                        Type=ActionTypes.OpenUrl,
                        Image = "https://image",
                        Value="https://value"
                    }
                }
            };

            var activity = Activity.CreateMessageActivity() as Activity;
            activity.Attachments.Add(new Attachment() { ContentType = SigninCard.ContentType, Content = signinCard });

            var skillResponse = ExecuteActivityToResponse(mapper, new MergedActivityResult { MergedActivity = activity }, skillRequest);

            Assert.NotNull(skillResponse.Response.Card);
            Assert.Equal(typeof(LinkAccountCard), skillResponse.Response.Card.GetType());
        }

        [Fact]
        public void TaskRequest()
        {
            var request = new LaunchRequest
            {
                Locale = "en-US",
                RequestId = "amzn1.echo-api.request.00000000-0000-0000-0000-000000000000",
                Type = "LaunchRequest",
                Timestamp = DateTime.Now,
                Task = new LaunchRequestTask
                {
                    Name = "AMAZON.PrintPDF",
                    Version = "1",
                    Input = new PrintPdfV1
                    {
                        Title = "Flywheel",
                        Description = "Flywheel",
                        Url = "http://www.example.com/flywheel.pdf"
                    }
                }
            };

            var skillRequest = new SkillRequest
            {
                Request = request,
                Context = new Context
                {
                    System = new AlexaSystem() { Application = new Application(), User = new User() }
                },
                Session = new Session()
                {
                    User = new User()
                }
            };

            var alexaMapper = new AlexaRequestMapper();
            var result = alexaMapper.RequestToActivity(skillRequest);

            JsonConvert.SerializeObject(result,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                Formatting = Formatting.Indented
                            });
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
            Assert.Null(activity.Conversation.ConversationType);
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

        private static void VerifyCardAttachmentAndDirectiveResponse(SkillResponse skillResponse, ICard card, IList<IDirective> directives)
        {
            card.IsSameOrEqualTo(skillResponse.Response.Card);
            Assert.Equal(directives.Count, skillResponse.Response.Directives.Count);
            directives.IsSameOrEqualTo(skillResponse.Response.Directives);
        }

        private static SkillResponse ExecuteActivityToResponse(AlexaRequestMapper mapper, MergedActivityResult mergedActivityResult, SkillRequest alexaRequest)
        {
            mergedActivityResult.MergedActivity = ActivityHelper.GetAnonymizedActivity(mergedActivityResult.MergedActivity);
            return mapper.ActivityToResponse(mergedActivityResult, alexaRequest);
        }
    }
}
