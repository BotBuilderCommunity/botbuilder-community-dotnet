using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Builder.Community.TranscriptStore.Tests
{
    public class TranscriptStoreBaseTests
    {
        protected async Task LogSingleActivityTest(ITranscriptStore transcriptStore)
        {
            var activity = new Activity();
            activity.ChannelId = "TestChannelId";
            activity.Conversation = new ConversationAccount();
            activity.Conversation.Id = "TestConversationId";
            activity.Timestamp = DateTimeOffset.Now;

            // Log activity.
            await transcriptStore.LogActivityAsync(activity);
        }

        protected async Task LogMultipleActivitiesTest(ITranscriptStore transcriptStore)
        {
            var activity = new Activity();
            activity.ChannelId = "TestChannelId";
            activity.Conversation = new ConversationAccount();
            activity.Conversation.Id = "TestConversationId";
            activity.Timestamp = DateTimeOffset.Now;

            for (int i = 0; i < 50; i++)
            {
                // Log activity.
                await transcriptStore.LogActivityAsync(activity);

                // Update timestamp.
                activity.Timestamp = DateTimeOffset.Now.AddSeconds(1);
            }
        }

        protected async Task RetrieveTranscriptsTest(ITranscriptStore transcriptStore)
        {
            // Arrange
            var activity = new Activity();
            activity.ChannelId = "TestChannelId";
            activity.Conversation = new ConversationAccount();
            activity.Conversation.Id = "TestConversationId";
            activity.Timestamp = DateTimeOffset.Now;

            for (int i = 0; i < 50; i++)
            {
                // Log activity.
                await transcriptStore.LogActivityAsync(activity);

                // Update timestamp.
                activity.Timestamp = DateTimeOffset.Now.AddSeconds(1);
            }

            // Act
            var result = new List<IActivity>();
            var pagedResult = new PagedResult<IActivity>();
            do
            {
                pagedResult = await transcriptStore.GetTranscriptActivitiesAsync(activity.ChannelId, activity.Conversation.Id, pagedResult.ContinuationToken);

                foreach (var item in pagedResult.Items)
                {
                    result.Add(item);
                }

                Assert.AreNotEqual(0, pagedResult.Items.Length);
            }
            while (pagedResult.ContinuationToken != null);

            // Assert
            Assert.AreNotEqual(0, result.Count);
        }

        protected async Task ListTranscriptsTest(ITranscriptStore transcriptStore)
        {
            // Arrange
            var activity = new Activity();
            activity.ChannelId = "TestChannelId";
            activity.Conversation = new ConversationAccount();
            activity.Conversation.Id = "TestConversationId";
            activity.Timestamp = DateTimeOffset.Now;

            for (int i = 0; i < 50; i++)
            {
                // Log activity.
                await transcriptStore.LogActivityAsync(activity);

                // Update timestamp.
                activity.Timestamp = DateTimeOffset.Now.AddSeconds(1);
            }

            // Act
            var result = new List<TranscriptInfo>();
            var pagedResult = new PagedResult<TranscriptInfo>();
            do
            {
                pagedResult = await transcriptStore.ListTranscriptsAsync(activity.ChannelId, pagedResult.ContinuationToken);

                foreach (var item in pagedResult.Items)
                {
                    result.Add(item);
                }

                Assert.AreNotEqual(0, pagedResult.Items.Length);
            }
            while (pagedResult.ContinuationToken != null);

            // Assert
            Assert.AreNotEqual(0, result.Count);
        }

        protected async Task DeleteTranscriptsTest(ITranscriptStore transcriptStore)
        {
            // Arrange
            var activity = new Activity();
            activity.ChannelId = "TestChannelId";
            activity.Conversation = new ConversationAccount();
            activity.Conversation.Id = "TestConversationId";
            activity.Timestamp = DateTimeOffset.Now;

            for (int i = 0; i < 50; i++)
            {
                // Log activity.
                await transcriptStore.LogActivityAsync(activity);

                // Update timestamp.
                activity.Timestamp = DateTimeOffset.Now.AddSeconds(1);
            }

            // Act
            await transcriptStore.DeleteTranscriptAsync(activity.ChannelId, activity.Conversation.Id);

            var result = new List<IActivity>();
            var pagedResult = new PagedResult<IActivity>();
            do
            {
                pagedResult = await transcriptStore.GetTranscriptActivitiesAsync(activity.ChannelId, activity.Conversation.Id, pagedResult.ContinuationToken);

                foreach (var item in pagedResult.Items)
                {
                    result.Add(item);
                }
            }
            while (pagedResult.ContinuationToken != null);

            // Assert
            Assert.AreEqual(0, result.Count);
        }
    }
}
