using AdaptiveCards;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Shared.Tests.TestUtilities
{
    public static class AttachmentUtility
    {
        public static Attachment ToAttachment(this NotSupportedCard card) => CreateAttachment(card, NotSupportedCard.ContentType);

        #region Missing from Bot Builder Sdk

        public static Attachment ToAttachment(this AdaptiveCard card) => CreateAttachment(card, AdaptiveCard.ContentType);

        #endregion Missing from Bot Builder Sdk

        private static Attachment CreateAttachment<T>(T card, string contentType)
        {
            return new Attachment
            {
                Content = card,
                ContentType = contentType
            };
        }
    }
}
