using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal static class TreeNodes
    {
        /// <summary>
        /// Gets the ID node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="PayloadItem"/>.
        /// </value>
        public static ITreeNode Id { get; } = new TreeNode<PayloadItem, object>((id, _) => Task.FromResult(id));

        /// <summary>
        /// Gets the payload node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="object"/>, because a payload can be deserialized as a <see cref="JObject"/> or a custom type.
        /// </value>
        public static ITreeNode Payload { get; } = new TreeNode<object, PayloadItem>(async (payload, nextAsync) =>
        {
            return await payload.ToJObjectAndBackAsync(
                async payloadJObject =>
                {
                    foreach (var type in PayloadIdTypes.Collection)
                    {
                        var id = payloadJObject.GetIdFromPayload(type);

                        if (id != null)
                        {
                            await nextAsync(new PayloadItem(type, id), Id);
                        }
                    }
                }, true).ConfigureAwait(false);
        });

        /// <summary>
        /// Gets the submit action node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="object"/>, because a submit action in an Adaptive Card
        /// can be deserialized as a <see cref="JObject"/> or a custom type.
        /// </value>
        public static ITreeNode SubmitAction { get; } = new TreeNode<object, JObject>(async (action, nextAsync) =>
        {
            // If the entry point was the Adaptive Card or higher
            // then the action will already be a JObject
            return await action.ToJObjectAndBackAsync(
                async actionJObject =>
                {
                    if (actionJObject.GetValue(CardConstants.KeyData) is JObject data)
                    {
                        await nextAsync(data, Payload).ConfigureAwait(false);
                    }
                }, true).ConfigureAwait(false);
        });

        /// <summary>
        /// Gets the card action node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.CardAction"/>.
        /// </value>
        public static ITreeNode CardAction { get; } = new TreeNode<CardAction, object>(async (action, nextAsync) =>
        {
            if (action.Type == ActionTypes.MessageBack || action.Type == ActionTypes.PostBack)
            {
                if (action.Value.ToJObject(true) != null)
                {
                    // This may end up converting the value to a JObject twice,
                    // but it's necessary so that the original non-JObject value
                    // can be reassigned without breaking the reference
                    action.Value = await nextAsync(action.Value, Payload).ConfigureAwait(false);
                }
                else
                {
                    // Strings are copied by value so no concern is needed
                    // about breaking this reference
                    action.Text = await action.Text.ToJObjectAndBackAsync(
                        async jObject => await nextAsync(jObject, Payload).ConfigureAwait(false),
                        true).ConfigureAwait(false);
                }
            }

            return action;
        });

        /// <summary>
        /// Gets the submit action list node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="IEnumerable{}">IEnumerable</see>&lt;<see cref="object"/>&gt;.
        /// </value>
        public static ITreeNode SubmitActionList { get; } = new EnumerableTreeNode<object>(SubmitAction, PayloadIdTypes.Card);

        /// <summary>
        /// Gets the card action list node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="IEnumerable{}">IEnumerable</see>&lt;<see cref="Microsoft.Bot.Schema.CardAction">CardAction</see>&gt;.
        /// </value>
        public static ITreeNode CardActionList { get; } = new EnumerableTreeNode<CardAction>(CardAction, PayloadIdTypes.Card);

        /// <summary>
        /// Gets the Adaptive Card node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="object"/>, because an Adaptive Card can be deserialized as a <see cref="JObject"/> or a custom type.
        /// </value>
        public static ITreeNode AdaptiveCard { get; } = new TreeNode<object, IEnumerable<JObject>>(async (card, nextAsync) =>
        {
            // Return the new object after it's been converted to a JObject and back
            // so that the attachment node can assign it back to the Content property
            return await card.ToJObjectAndBackAsync(
                async cardJObject =>
                {
                    await nextAsync(
                        AdaptiveCardUtil.NonDataDescendants(cardJObject)
                            .Select(token => token is JObject element
                                    && element.GetValue(CardConstants.KeyType) is JToken type
                                    && type.Type == JTokenType.String
                                    && type.ToString().Equals(CardConstants.ActionSubmit)
                                ? element : null)
                            .WhereNotNull(), SubmitActionList).ConfigureAwait(false);
                }, true).ConfigureAwait(false);
        });

        public static ITreeNode AnimationCard { get; } = new RichCardTreeNode<AnimationCard>(card => card.Buttons);

        public static ITreeNode AudioCard { get; } = new RichCardTreeNode<AudioCard>(card => card.Buttons);

        public static ITreeNode HeroCard { get; } = new RichCardTreeNode<HeroCard>(card => card.Buttons);

        public static ITreeNode OAuthCard { get; } = new RichCardTreeNode<OAuthCard>(card => card.Buttons);

        public static ITreeNode ReceiptCard { get; } = new RichCardTreeNode<ReceiptCard>(card => card.Buttons);

        public static ITreeNode SigninCard { get; } = new RichCardTreeNode<SigninCard>(card => card.Buttons);

        public static ITreeNode ThumbnailCard { get; } = new RichCardTreeNode<ThumbnailCard>(card => card.Buttons);

        public static ITreeNode VideoCard { get; } = new RichCardTreeNode<VideoCard>(card => card.Buttons);

        /// <summary>
        /// Gets the attachment node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.Attachment"/>.
        /// </value>
        public static ITreeNode Attachment { get; } = new TreeNode<Attachment, object>(async (attachment, nextAsync) =>
        {
            var contentType = attachment.ContentType;

            if (contentType != null && CardTypes.ContainsKey(contentType))
            {
                // The nextAsync return value is needed here because the attachment could be an Adaptive Card
                // which would mean a new object was generated by the JObject conversion/deconversion
                attachment.Content = await nextAsync(attachment.Content, CardTypes[contentType]).ConfigureAwait(false);
            }

            return attachment;
        });

        /// <summary>
        /// Gets the carousel node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="IEnumerable{}">IEnumerable</see>&lt;<see cref="Microsoft.Bot.Schema.Attachment">Attachment</see>&gt;.
        /// The name of this node type is not meant to imply that it only works when an activity's
        /// <see cref="Activity.AttachmentLayout">AttachmentLayout</see> property is assigned the value of
        /// <see cref="AttachmentLayoutTypes.Carousel"/>. In this context, a "carousel" is just a set of attachments regardless of layout.
        /// </value>
        public static ITreeNode Carousel { get; } = new EnumerableTreeNode<Attachment>(Attachment, PayloadIdTypes.Carousel);

        /// <summary>
        /// Gets the activity node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.Activity"/>.
        /// </value>
        public static ITreeNode Activity { get; } = new TreeNode<IMessageActivity, IEnumerable<Attachment>>(async (activity, nextAsync) =>
        {
            // The nextAsync return value is not needed here because the Attachments property reference will remain unchanged
            await nextAsync(activity.Attachments, Carousel).ConfigureAwait(false);

            return activity;
        });

        /// <summary>
        /// Gets the batch node.
        /// </summary>
        /// <value>
        /// Corresponds to <see cref="IEnumerable{}">IEnumerable</see>&lt;<see cref="Microsoft.Bot.Schema.Activity">Activity</see>&gt;.
        /// </value>
        public static ITreeNode Batch { get; } = new EnumerableTreeNode<IMessageActivity>(Activity, PayloadIdTypes.Batch);

        internal static IReadOnlyCollection<ITreeNode> Collection { get; } = Array.AsReadOnly(new[]
        {
            Batch,
            Activity,
            Carousel,
            Attachment,
            AdaptiveCard,
            AnimationCard,
            AudioCard,
            HeroCard,
            OAuthCard,
            ReceiptCard,
            SigninCard,
            ThumbnailCard,
            VideoCard,
            SubmitActionList,
            CardActionList,
            SubmitAction,
            CardAction,
            Payload,
            Id,
        });

        private static Dictionary<string, ITreeNode> CardTypes { get; } = new Dictionary<string, ITreeNode>(StringComparer.OrdinalIgnoreCase)
        {
            { CardConstants.AdaptiveCardContentType, AdaptiveCard },
            { Microsoft.Bot.Schema.AnimationCard.ContentType, AnimationCard },
            { Microsoft.Bot.Schema.AudioCard.ContentType, AudioCard },
            { Microsoft.Bot.Schema.HeroCard.ContentType, HeroCard },
            { Microsoft.Bot.Schema.ReceiptCard.ContentType, ReceiptCard },
            { Microsoft.Bot.Schema.SigninCard.ContentType, SigninCard },
            { Microsoft.Bot.Schema.OAuthCard.ContentType, OAuthCard },
            { Microsoft.Bot.Schema.ThumbnailCard.ContentType, ThumbnailCard },
            { Microsoft.Bot.Schema.VideoCard.ContentType, VideoCard },
        };
    }
}
