using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Handoff.Channel.Helpers
{
    public static class ConversationReferenceHelpers
    {
        /// <summary>
        /// Checks if the given conversation reference contains the channel account instance of a bot.
        /// </summary>
        /// <param name="conversationReference">The conversation reference to check.</param>
        /// <returns>True, if the given conversation reference is associated with a bot. False otherwise.</returns>
        public static bool IsBot(this ConversationReference conversationReference)
        {
            return (conversationReference?.Bot != null);
        }

        /// <summary>
        /// Resolves the non-null channel account instance in the given conversation reference.
        /// </summary>
        /// <param name="conversationReference">The conversation reference whose channel account to resolve.</param>
        /// <returns>The non-null channel account instance (user or bot) or null, if both are null.</returns>
        public static ChannelAccount GetChannelAccount(this ConversationReference conversationReference)
        {
            return conversationReference.User ?? conversationReference.Bot;
        }

        /// <summary>
        /// Compares the conversation and channel account instances of the two given conversation references.
        /// </summary>
        /// <param name="conversationReference1"></param>
        /// <param name="conversationReference2"></param>
        /// <returns>True, if the IDs match. False otherwise.</returns>
        public static bool Match(this ConversationReference conversationReference1, ConversationReference conversationReference2)
        {
            if (conversationReference1 == null || conversationReference2 == null)
            {
                return false;
            }

            string conversationAccount1Id = conversationReference1.Conversation?.Id;
            string conversationAccount2Id = conversationReference2.Conversation?.Id;

            if (string.IsNullOrWhiteSpace(conversationAccount1Id) != string.IsNullOrWhiteSpace(conversationAccount2Id))
            {
                return false;
            }

            bool conversationAccountsMatch =
                (string.IsNullOrWhiteSpace(conversationAccount1Id)
                 && string.IsNullOrWhiteSpace(conversationAccount2Id))
                || conversationAccount1Id.Equals(conversationAccount2Id);

            var channelAccount1 = GetChannelAccount(conversationReference1);
            var channelAccount2 = GetChannelAccount(conversationReference2);

            bool channelAccountsMatch =
                IsBot(conversationReference1) == IsBot(conversationReference2)
                && ((channelAccount1 == null && channelAccount2 == null)
                || (channelAccount1 != null && channelAccount2 != null
                && channelAccount1.Id.Equals(channelAccount2.Id)));

            return (conversationAccountsMatch && channelAccountsMatch);
        }
        
        /// <summary>
        /// Constructs a conversation reference instance using the sender of the given activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="senderIsBot">Defines whether to classify the sender as a bot or a user.</param>
        /// <returns>A newly created conversation reference instance.</returns>
        public static ConversationReference CreateSenderConversationReference(this IActivity activity, bool senderIsBot = false)
        {
            return new ConversationReference(
                null,
                senderIsBot ? null : activity.From,
                senderIsBot ? activity.From : null,
                activity.Conversation,
                activity.ChannelId,
                activity.ServiceUrl);
        }

        /// <summary>
        /// Constructs a conversation reference instance using the recipient, which is expected to
        /// be a bot instance, of the given activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>A newly created conversation reference instance.</returns>
        public static ConversationReference CreateRecipientConversationReference(this IActivity activity)
        {
            return new ConversationReference(
                null,
                null,
                activity.Recipient,
                activity.Conversation,
                activity.ChannelId,
                activity.ServiceUrl);
        }
    }
}
