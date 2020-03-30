using System;
using System.Linq;
using Bot.Builder.Community.Adapters.RingCentral.Helpers;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Bot.Builder.Community.Adapters.RingCentral.Tests
{
    public class RingCentralSDKHelperTests
    {
        [Fact]
        public void InitializeRingCentralConfigurationShouldReturnWithAuthorisationHeader()
        {
            // Arrange
            var token = "123xyz";
            var expectedBearerToken = $"Bearer {token}";

            // Act
            var config = RingCentralSdkHelper.InitializeRingCentralConfiguration("url", token);

            // Assert
            Assert.True(config.DefaultHeaders.Count > 0);
            Assert.Equal(config.DefaultHeaders["Authorization"], expectedBearerToken);
        }

        [Fact]
        public void InitializeRingCentralConfigurationShouldReturnWithBasePath()
        {
            // Arrange
            var token = "123xyz";
            var expectedBasePath = "http://someurl";

            // Act
            var config = RingCentralSdkHelper.InitializeRingCentralConfiguration(expectedBasePath, token);

            // Assert
            Assert.Equal(config.BasePath, expectedBasePath);
        }

        [Fact]
        public void ForeignThreadIDFromActivityReturnsWithFormattedIdentifier()
        {
            // Arrange
            string conversationId = "convId";
            string channelId = "channelId";
            string serviceUrl = "http://someserviceurl.com";
            string expectedForeignThreadId = $"{conversationId}_{channelId}_{serviceUrl}";

            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Conversation = new ConversationAccount(id: conversationId);
            activity.Object.ChannelId = channelId;
            activity.Object.ServiceUrl = serviceUrl;

            // Act
            var foreignThreadId = RingCentralSdkHelper.BuildForeignThreadIdFromActivity(activity.Object);

            // Assert
            Assert.Equal(expectedForeignThreadId, foreignThreadId);
        }

        [Fact]
        public void ConversationReferenceFromForeignThreadReturnsValidConversationReference()
        {
            // Arrange
            string expectedConversationId = "convId";
            string expectedBotId = "testBot";
            string channelId = "channelId";
            string expectedServiceUrl = "http://someserviceurl.com";
            string foreignThreadId = $"{expectedConversationId}_{channelId}_{expectedServiceUrl}";

            // Act
            var conversationReference = RingCentralSdkHelper.ConversationReferenceFromForeignThread(foreignThreadId, expectedBotId);

            // Assert
            Assert.Equal(expectedConversationId, conversationReference.Conversation.Id);
            Assert.Equal(expectedServiceUrl, conversationReference.ServiceUrl);
            Assert.Equal(expectedBotId, conversationReference.Bot.Id);
        }

        [Fact]
        public void ConversationReferenceFromIncompleteForeignThreadShouldFailWithOutOfRange()
        {
            // Arrange
            string expectedConversationId = "convId";
            string expectedBotId = "testBot";
            string channelId = "channelId";
            string foreignThreadId = $"{expectedConversationId}_{channelId}";

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() => { RingCentralSdkHelper.ConversationReferenceFromForeignThread(foreignThreadId, expectedBotId); });
        }

        [Fact]
        public void RingCentralResponseActivityReturnsHumanHandOffActivity()
        {
            // Arrange
            string expectedMessage = "test agent message";

            // Act
            var activity = RingCentralSdkHelper.RingCentralAgentResponseActivity(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, activity.Text);
            Assert.True(activity.Entities.Any());
            Assert.True(activity.Entities.FirstOrDefault().GetAs<Schema.RingCentralMetadata>().IsHumanResponse == true);
        }
    }
}
