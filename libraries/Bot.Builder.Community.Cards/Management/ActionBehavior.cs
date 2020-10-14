using Bot.Builder.Community.Cards.Management.Tree;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Bot.Builder.Community.Cards.Management
{
    // TODO: Write tests for these methods
    public static class ActionBehavior
    {
        public static void SetInBatch(IEnumerable<IMessageActivity> batch, string name, object value) => Set(batch, name, value, TreeNodeType.Batch);

        public static void SetInActivity(IMessageActivity activity, string name, object value) => Set(activity, name, value, TreeNodeType.Activity);

        public static void SetInCarousel(IEnumerable<Attachment> carousel, string name, object value) => Set(carousel, name, value, TreeNodeType.Carousel);

        public static void SetInAttachment(Attachment attachment, string name, object value) => Set(attachment, name, value, TreeNodeType.Attachment);

        public static void SetInAdaptiveCard(ref object card, string name, object value) => card = Set(card, name, value, TreeNodeType.AdaptiveCard);

        public static void SetInAnimationCard(AnimationCard card, string name, object value) => Set(card, name, value, TreeNodeType.AnimationCard);

        public static void SetInAudioCard(AudioCard card, string name, object value) => Set(card, name, value, TreeNodeType.AudioCard);

        public static void SetInHeroCard(HeroCard card, string name, object value) => Set(card, name, value, TreeNodeType.HeroCard);

        public static void SetInOAuthCard(OAuthCard card, string name, object value) => Set(card, name, value, TreeNodeType.OAuthCard);

        public static void SetInReceiptCard(ReceiptCard card, string name, object value) => Set(card, name, value, TreeNodeType.ReceiptCard);

        public static void SetInSigninCard(SigninCard card, string name, object value) => Set(card, name, value, TreeNodeType.SigninCard);

        public static void SetInThumbnailCard(ThumbnailCard card, string name, object value) => Set(card, name, value, TreeNodeType.ThumbnailCard);

        public static void SetInVideoCard(VideoCard card, string name, object value) => Set(card, name, value, TreeNodeType.VideoCard);

        public static void SetInSubmitAction(ref object action, string name, object value) => action = Set(action, name, value, TreeNodeType.SubmitAction);

        public static void SetInCardAction(CardAction action, string name, object value) => Set(action, name, value, TreeNodeType.CardAction);

        public static void SetInActionData(ref object data, string name, object value) => data = Set(data, name, value, TreeNodeType.ActionData);

        private static T Set<T>(
                T entryValue,
                string behaviorName,
                object behaviorValue,
                TreeNodeType entryType)
            where T : class
        {
            if (behaviorName is null)
            {
                throw new ArgumentNullException(nameof(behaviorName));
            }

            var jToken = behaviorValue is null ? null : JToken.FromObject(behaviorValue);

            return CardTree.SetLibraryData(entryValue, new JObject { { behaviorName, jToken } }, entryType, true);
        }
    }
}
