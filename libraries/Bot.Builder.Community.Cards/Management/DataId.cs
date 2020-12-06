using Bot.Builder.Community.Cards.Management.Tree;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bot.Builder.Community.Cards.Management
{
    public class DataId : DataItem<string>
    {
        public static void SetInBatch(IEnumerable<IMessageActivity> batch, DataIdOptions options = null) => CardTree.ApplyIds(batch, options, TreeNodeType.Batch);

        public static void SetInActivity(IMessageActivity activity, DataIdOptions options = null) => CardTree.ApplyIds(activity, options, TreeNodeType.Activity);

        public static void SetInCarousel(IEnumerable<Attachment> carousel, DataIdOptions options = null) => CardTree.ApplyIds(carousel, options, TreeNodeType.Carousel);

        public static void SetInAttachment(Attachment attachment, DataIdOptions options = null) => CardTree.ApplyIds(attachment, options, TreeNodeType.Attachment);

        public static void SetInAdaptiveCard(ref object card, DataIdOptions options = null) => card = CardTree.ApplyIds(card, options, TreeNodeType.AdaptiveCard);

        public static void SetInAnimationCard(AnimationCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options, TreeNodeType.AnimationCard);

        public static void SetInAudioCard(AudioCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options, TreeNodeType.AudioCard);

        public static void SetInHeroCard(HeroCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options, TreeNodeType.HeroCard);

        public static void SetInOAuthCard(OAuthCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options, TreeNodeType.OAuthCard);

        public static void SetInReceiptCard(ReceiptCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options, TreeNodeType.ReceiptCard);

        public static void SetInSigninCard(SigninCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options, TreeNodeType.SigninCard);

        public static void SetInThumbnailCard(ThumbnailCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options, TreeNodeType.ThumbnailCard);

        public static void SetInVideoCard(VideoCard card, DataIdOptions options = null) => CardTree.ApplyIds(card, options, TreeNodeType.VideoCard);

        public static void SetInSubmitAction(ref object action, DataIdOptions options = null) => action = CardTree.ApplyIds(action, options, TreeNodeType.SubmitAction);

        public static void SetInCardAction(CardAction action, DataIdOptions options = null) => CardTree.ApplyIds(action, options, TreeNodeType.CardAction);

        public static void SetInActionData(ref object data, DataIdOptions options = null) => data = CardTree.ApplyIds(data, options, TreeNodeType.ActionData);

        /// <summary>
        /// An action data ID that can identify the action, card, carousel, or batch the data came from.
        /// </summary>
        public DataId(string scope, string value) : base(scope, value)
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
