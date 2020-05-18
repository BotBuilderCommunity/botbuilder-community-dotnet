using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal enum TreeNodeType
    {
        /// <summary>
        /// Corresponds to <see cref="IEnumerable{T}">IEnumerable</see>&lt;<see cref="Microsoft.Bot.Schema.Activity">Activity</see>&gt;.
        /// </summary>
        Batch,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.Activity"/>.
        /// </summary>
        Activity,

        /// <summary>
        /// Corresponds to <see cref="IEnumerable{T}">IEnumerable</see>&lt;<see cref="Microsoft.Bot.Schema.Attachment">Attachment</see>&gt;.
        /// The name of this node type is not meant to imply that it only works when an activity's
        /// <see cref="Activity.AttachmentLayout">AttachmentLayout</see> property is assigned the value of
        /// <see cref="AttachmentLayoutTypes.Carousel"/>. In this context, a "carousel" is just a set of attachments regardless of layout.
        /// </summary>
        Carousel,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.Attachment"/>.
        /// </summary>
        Attachment,

        /// <summary>
        /// Corresponds to <see cref="object"/>, because an Adaptive Card can be deserialized as a <see cref="JObject"/> or a custom type.
        /// </summary>
        AdaptiveCard,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.AnimationCard"/>.
        /// </summary>
        AnimationCard,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.AudioCard"/>.
        /// </summary>
        AudioCard,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.HeroCard"/>.
        /// </summary>
        HeroCard,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.OAuthCard"/>.
        /// </summary>
        OAuthCard,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.ReceiptCard"/>.
        /// </summary>
        ReceiptCard,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.SigninCard"/>.
        /// </summary>
        SigninCard,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.ThumbnailCard"/>.
        /// </summary>
        ThumbnailCard,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.VideoCard"/>.
        /// </summary>
        VideoCard,

        /// <summary>
        /// Corresponds to <see cref="IEnumerable{T}">IEnumerable</see>&lt;<see cref="object"/>&gt;.
        /// </summary>
        SubmitActionList,

        /// <summary>
        /// Corresponds to <see cref="IEnumerable{T}">IEnumerable</see>&lt;<see cref="Microsoft.Bot.Schema.CardAction">CardAction</see>&gt;.
        /// </summary>
        CardActionList,

        /// <summary>
        /// Corresponds to <see cref="object"/>, because a submit action in an Adaptive Card
        /// can be deserialized as a <see cref="JObject"/> or a custom type.
        /// </summary>
        SubmitAction,

        /// <summary>
        /// Corresponds to <see cref="Microsoft.Bot.Schema.CardAction"/>.
        /// </summary>
        CardAction,

        /// <summary>
        /// Corresponds to <see cref="object"/>, because action data can be deserialized as a <see cref="JObject"/> or a custom type.
        /// </summary>
        ActionData,

        /// <summary>
        /// Corresponds to <see cref="DataId"/>.
        /// </summary>
        Id,
    }
}
