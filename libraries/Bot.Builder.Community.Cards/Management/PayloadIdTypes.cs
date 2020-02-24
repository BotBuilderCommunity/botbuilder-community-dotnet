using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Cards.Management
{
    /// <summary>
    /// This class defines ID types with progressively increasing scope sizes.
    /// </summary>
    public static class PayloadIdTypes
    {
        /// <summary>
        /// An action ID should be globally unique and not found in different actions.
        /// </summary>
        public const string Action = "action";

        /// <summary>
        /// A card ID should be the same for every action in a single card attachment.
        /// </summary>
        public const string Card = "card";

        /// <summary>
        /// A carousel ID should be the same for every action across all card attachments
        /// in a single activity. This is not called an activity ID to avoid confusion because
        /// that would be ambiguous with an activity's actual activity ID from the channel.
        /// Activities with a "list" attachment layout can still use this.
        /// </summary>
        public const string Carousel = "carousel";

        /// <summary>
        /// A batch ID should be the same for every action in a "batch" of activities.
        /// A batch of activities normally refers to the activities sent in a call to
        /// <see cref="ITurnContext.SendActivitiesAsync">SendActivitiesAsync</see>
        /// but this can be used for any arbitrarily-defined group of activities.
        /// </summary>
        public const string Batch = "batch";

        internal static IList<string> Collection { get; } = Array.AsReadOnly(new[] { Action, Card, Carousel, Batch });

        internal static int GetIndex(string type) => Collection.IndexOf(type);

        internal static string GetKey(string type) => $"{CardConstants.PackageId}{type}Id";

        internal static string GenerateId(string type) => $"{type}-{Guid.NewGuid()}";
    }
}