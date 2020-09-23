using Bot.Builder.Community.Cards.Management.Tree;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bot.Builder.Community.Cards.Management
{
    public class DataId : DataItem<string>
    {
        public static void SetForBatch(IEnumerable<IMessageActivity> batch, DataIdOptions options = null) => CardTree.ApplyIds(batch, options);

        public static void SetForActivity(IMessageActivity activity, DataIdOptions options = null) => CardTree.ApplyIds(activity, options);

        public static void SetForCarousel(IEnumerable<Attachment> carousel, DataIdOptions options = null) => CardTree.ApplyIds(carousel, options);

        public static void SetForAttachment(Attachment attachment, DataIdOptions options = null) => CardTree.ApplyIds(attachment, options);

        public static void SetForAdaptiveCard(object card, DataIdOptions options = null) => CardTree.ApplyIds(card, options, TreeNodeType.AdaptiveCard);

        public static void SetForAnimationCard(AnimationCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options);

        public static void SetForAudioCard(AudioCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options);

        public static void SetForHeroCard(HeroCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options);

        public static void SetForOAuthCard(OAuthCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options);

        public static void SetForReceiptCard(ReceiptCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options);

        public static void SetForSigninCard(SigninCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options);

        public static void SetForThumbnailCard(ThumbnailCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options);

        public static void SetForVideoCard(VideoCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options);

        public static void SetForSubmitAction(object action, DataIdOptions options = null) => CardTree.ApplyIds(action, options, TreeNodeType.SubmitAction);

        public static void SetForCardAction(CardAction action, DataIdOptions options = null) => CardTree.ApplyIds(action, options);

        public static void SetForActionData(object data, DataIdOptions options = null) => CardTree.ApplyIds(data, options, TreeNodeType.ActionData);

        /// <summary>
        /// An action data ID that can identify the action, card, carousel, or batch the data came from.
        /// </summary>
        public DataId(string key, string value) : base(key, value)
        {
        }

        internal static ReadOnlyCollection<string> Scopes { get; } = Array.AsReadOnly(new[]
        {
            DataIdScopes.Action,
            DataIdScopes.Card,
            DataIdScopes.Carousel,
            DataIdScopes.Batch,
        });

        internal static string GenerateValue(string scope) => $"{scope}-{Guid.NewGuid()}";
    }
}
