using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards
{
    public static class CardExtensions
    {
        public static string ApplyIdToBatch(this List<Activity> activities, IdOptions options = null)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            if (activities.Any())
            {
                string batchId = IdType.Batch.ReplaceNullWithId(ref options);
                string attachmentsId = null;

                foreach (var activity in activities)
                {
                    attachmentsId = activity.ApplyIdToAttachments(options);
                }

                return batchId ?? attachmentsId;
            }

            return null;
        }

        public static string ApplyIdToAttachments(this Activity activity, IdOptions options = null)
        {
            if (activity is null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (activity.Attachments != null && activity.Attachments.Any())
            {
                string attachmentsId = IdType.Attachments.ReplaceNullWithId(ref options);
                string cardId = null;

                foreach (var attachment in activity.Attachments)
                {
                    cardId = attachment.ApplyId(options);
                }

                return attachmentsId ?? cardId;
            }

            return null;
        }

        public static string GetIdFromPayload(this JObject payload, IdType type = IdType.Card)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var token = payload[type.GetKey()];

            return token?.Type == JTokenType.String ? token.ToString() : null;
        }

        public static string ApplyIdToPayload(this JObject payload, IdOptions options = null)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (options is null)
            {
                options = new IdOptions(IdType.Action);
            }

            string actionId = null;

            foreach (var kvp in options.GetIds())
            {
                if (options.Overwrite || payload.GetIdFromPayload(kvp.Key) is null)
                {
                    var id = kvp.Value;

                    if (id is null)
                    {
                        if (kvp.Key == IdType.Action)
                        {
                            // Only generate an ID for the action
                            id = IdType.Action.GenerateId();
                        }
                        else
                        {
                            // If any other ID's are null, just ignore them
                            continue;
                        }
                    }

                    if (kvp.Key == IdType.Action)
                    {
                        actionId = id;
                    }

                    payload[IdType.Card.GetKey()] = id;
                }
            }

            return actionId;
        }

        public static string ApplyIdToAdaptiveCard(this JObject adaptiveCard, IdOptions options = null)
        {
            if (adaptiveCard is null)
            {
                throw new ArgumentNullException(nameof(adaptiveCard));
            }

            var cardId = IdType.Card.ReplaceNullWithId(ref options);

            // Iterate over all objects named "data" that aren't nested in another object named "data".
            // This effectively finds all the "payloads" of submit actions while accounting for situations
            // where a submit action's data itself contains a property with the name "data".
            foreach (var data in adaptiveCard.NonDataDescendants()
                .Select(token => token as JProperty)
                .Where(prop =>
                    prop.Name.Equals(
                        CardConstants.KeyData,
                        StringComparison.OrdinalIgnoreCase)
                    && prop.Value?.Type == JTokenType.Object)
                .Select(prop => prop.Value as JObject))
            {
                data.ApplyIdToPayload(options);
            }

            return cardId;
        }

        public static string ApplyId(this AnimationCard card, IdOptions options = null)
        {
            if (card is null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return card.Buttons.ApplyId(options);
        }

        public static string ApplyId(this AudioCard card, IdOptions options = null)
        {
            if (card is null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return card.Buttons.ApplyId(options);
        }

        public static string ApplyId(this HeroCard card, IdOptions options = null)
        {
            if (card is null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return card.Buttons.ApplyId(options);
        }

        public static string ApplyId(this OAuthCard card, IdOptions options = null)
        {
            if (card is null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return card.Buttons.ApplyId(options);
        }

        public static string ApplyId(this ReceiptCard card, IdOptions options = null)
        {
            if (card is null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return card.Buttons.ApplyId(options);
        }

        public static string ApplyId(this SigninCard card, IdOptions options = null)
        {
            if (card is null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return card.Buttons.ApplyId(options);
        }

        public static string ApplyId(this ThumbnailCard card, IdOptions options = null)
        {
            if (card is null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return card.Buttons.ApplyId(options);
        }

        public static string ApplyId(this VideoCard card, IdOptions options = null)
        {
            if (card is null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return card.Buttons.ApplyId(options);
        }

        public static string ApplyId(this IList<CardAction> buttons, IdOptions options = null)
        {
            if (buttons is null)
            {
                throw new ArgumentNullException(nameof(buttons));
            }

            string cardId = IdType.Card.ReplaceNullWithId(ref options);
            string actionId = null;

            foreach (var button in buttons)
            {
                if ((button.Type == ActionTypes.MessageBack || button.Type == ActionTypes.PostBack) && button.Value is JObject value)
                {
                    actionId = value.ApplyIdToPayload(options);
                }
            }

            return cardId ?? actionId;
        }

        public static string ApplyId(this Attachment attachment, IdOptions options = null)
        {
            if (attachment is null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            switch (attachment.ContentType)
            {
                case CardConstants.AdaptiveCardContentType:
                    return JObject.FromObject(attachment.Content).ApplyIdToAdaptiveCard(options);
                case AnimationCard.ContentType:
                    return (attachment.Content as AnimationCard)?.ApplyId(options);
                case AudioCard.ContentType:
                    return (attachment.Content as AudioCard)?.ApplyId(options);
                case HeroCard.ContentType:
                    return (attachment.Content as HeroCard)?.ApplyId(options);
                case ReceiptCard.ContentType:
                    return (attachment.Content as ReceiptCard)?.ApplyId(options);
                case SigninCard.ContentType:
                    return (attachment.Content as SigninCard)?.ApplyId(options);
                case OAuthCard.ContentType:
                    return (attachment.Content as OAuthCard)?.ApplyId(options);
                case ThumbnailCard.ContentType:
                    return (attachment.Content as ThumbnailCard)?.ApplyId(options);
                case VideoCard.ContentType:
                    return (attachment.Content as VideoCard)?.ApplyId(options);
            }

            return null;
        }

        public static IEnumerable<JToken> NonDataDescendants(this JContainer container)
        {
            return container.Descendants().Where(token =>
                !token.Ancestors().Any(ancestor =>
                    (ancestor as JProperty)?.Name.Equals(
                        CardConstants.KeyData,
                        StringComparison.OrdinalIgnoreCase) == true));
        }

        internal static string GetKey(this IdType type)
        {
            // If multiple flags are present, only use the first one
            var typeString = type.ToString().Split(',').First();

            return $"{CardConstants.PackageId}{typeString}Id";
        }

        internal static string GenerateId(this IdType type) => $"{type}-{Guid.NewGuid()}";

        internal static string ReplaceNullWithId(this IdType type, ref IdOptions options)
        {
            options = options?.Clone() ?? new IdOptions();

            if (options.HasIdType(type))
            {
                var id = options.Get(type);

                return id is null ? options.Set(type, type.GenerateId()) : id;
            }

            return null;
        }
    }
}