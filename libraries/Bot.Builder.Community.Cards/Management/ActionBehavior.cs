using Bot.Builder.Community.Cards.Management.Tree;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Cards.Management
{
    public class ActionBehavior
    {
        public static void SetForBatch(IEnumerable<IMessageActivity> batch, string name, object value) => batch.SetActionBehavior(name, value);

        public static void SetForActivity(IMessageActivity activity, string name, object value) => activity.SetActionBehavior(name, value);

        public static void SetForCarousel(IEnumerable<Attachment> carousel, string name, object value) => carousel.SetActionBehavior(name, value);

        public static void SetForAttachment(Attachment attachment, string name, object value) => attachment.SetActionBehavior(name, value);

        public static void SetForAdaptiveCard(object card, string name, object value) => card.SetActionBehavior(name, value, TreeNodeType.AdaptiveCard);

        public static void SetForAnimationCard(AnimationCard card, string name, object value) => card.SetActionBehavior(name, value);

        public static void SetForAudioCard(AudioCard card, string name, object value) => card.SetActionBehavior(name, value);

        public static void SetForHeroCard(HeroCard card, string name, object value) => card.SetActionBehavior(name, value);

        public static void SetForOAuthCard(OAuthCard card, string name, object value) => card.SetActionBehavior(name, value);

        public static void SetForReceiptCard(ReceiptCard card, string name, object value) => card.SetActionBehavior(name, value);

        public static void SetForSigninCard(SigninCard card, string name, object value) => card.SetActionBehavior(name, value);

        public static void SetForThumbnailCard(ThumbnailCard card, string name, object value) => card.SetActionBehavior(name, value);

        public static void SetForVideoCard(VideoCard card, string name, object value) => card.SetActionBehavior(name, value);

        public static void SetForSubmitAction(object action, string name, object value) => action.SetActionBehavior(name, value, TreeNodeType.SubmitAction);

        public static void SetForCardAction(CardAction action, string name, object value) => action.SetActionBehavior(name, value);

        public static void SetForActionData(object data, string name, object value) => data.SetActionBehavior(name, value, TreeNodeType.ActionData);
    }
}
