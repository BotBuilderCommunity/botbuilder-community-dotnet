using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards
{
    public static class CardExtensions
    {
        public static RecursiveIdCollection ApplyIdsToBatch(this IEnumerable<Activity> activities, IdOptions options = null)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            var ids = new List<RecursiveIdCollection>();

            if (activities.Any())
            {
                IdType.Batch.ReplaceNullWithId(ref options);
                options.SetExistingId(IdType.Carousel);
                options.SetExistingId(IdType.Card);
                options.SetExistingId(IdType.Action);

                foreach (var activity in activities)
                {
                    ids.Add(activity.ApplyIdsToCarousel(options));
                }
            }

            return new RecursiveIdCollection(ids);
        }

        public static RecursiveIdCollection ApplyIdsToCarousel(this Activity activity, IdOptions options = null)
        {
            if (activity is null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var ids = new List<RecursiveIdCollection>();

            if (activity.Attachments?.Any() == true)
            {
                IdType.Carousel.ReplaceNullWithId(ref options);
                options.SetExistingId(IdType.Card);
                options.SetExistingId(IdType.Action);

                foreach (var attachment in activity.Attachments)
                {
                    ids.Add(attachment.ApplyIds(options) ?? new RecursiveIdCollection());
                }
            }

            return new RecursiveIdCollection(ids);
        }

        public static RecursiveIdCollection ApplyIds(this Attachment attachment, IdOptions options = null)
        {
            if (attachment is null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            switch (attachment.ContentType)
            {
                case CardConstants.AdaptiveCardContentType:
                    RecursiveIdCollection ids = null;

                    attachment.Content = attachment.Content?.ToJObjectAndBackAsync(
                        card =>
                        {
                            ids = card.ApplyIdsToAdaptiveCard(options);

                            return Task.CompletedTask;
                        }).Result;

                    return ids;
                case AnimationCard.ContentType:
                    return (attachment.Content as AnimationCard)?.ApplyIds(options);
                case AudioCard.ContentType:
                    return (attachment.Content as AudioCard)?.ApplyIds(options);
                case HeroCard.ContentType:
                    return (attachment.Content as HeroCard)?.ApplyIds(options);
                case ReceiptCard.ContentType:
                    return (attachment.Content as ReceiptCard)?.ApplyIds(options);
                case SigninCard.ContentType:
                    return (attachment.Content as SigninCard)?.ApplyIds(options);
                case OAuthCard.ContentType:
                    return (attachment.Content as OAuthCard)?.ApplyIds(options);
                case ThumbnailCard.ContentType:
                    return (attachment.Content as ThumbnailCard)?.ApplyIds(options);
                case VideoCard.ContentType:
                    return (attachment.Content as VideoCard)?.ApplyIds(options);
            }

            return null;
        }

        public static RecursiveIdCollection ApplyIdsToAdaptiveCard(this JObject adaptiveCard, IdOptions options = null)
        {
            if (adaptiveCard is null)
            {
                throw new ArgumentNullException(nameof(adaptiveCard));
            }

            var ids = new List<RecursiveIdCollection>();

            IdType.Card.ReplaceNullWithId(ref options);
            options.SetExistingId(IdType.Action);

            // Iterate over all objects named "data" that aren't nested in another object named "data".
            // This effectively finds all the "payloads" of submit actions while accounting for situations
            // where a submit action's data itself contains a property with the name "data".
            foreach (var data in adaptiveCard.NonDataDescendants()
                .Select(token => token as JProperty)
                .Where(prop =>
                    prop?.Name.Equals(
                        CardConstants.KeyData,
                        StringComparison.OrdinalIgnoreCase) == true
                    && prop.Value?.Type == JTokenType.Object)
                .Select(prop => prop.Value as JObject))
            {
                ids.Add(data.ApplyIdsToPayload(options));
            }

            return new RecursiveIdCollection(ids);
        }

        public static RecursiveIdCollection ApplyIds(this AnimationCard card, IdOptions options = null) => card?.Buttons?.ApplyIds(options);

        public static RecursiveIdCollection ApplyIds(this AudioCard card, IdOptions options = null) => card?.Buttons?.ApplyIds(options);

        public static RecursiveIdCollection ApplyIds(this HeroCard card, IdOptions options = null) => card?.Buttons?.ApplyIds(options);

        public static RecursiveIdCollection ApplyIds(this OAuthCard card, IdOptions options = null) => card?.Buttons?.ApplyIds(options);

        public static RecursiveIdCollection ApplyIds(this ReceiptCard card, IdOptions options = null) => card?.Buttons?.ApplyIds(options);

        public static RecursiveIdCollection ApplyIds(this SigninCard card, IdOptions options = null) => card?.Buttons?.ApplyIds(options);

        public static RecursiveIdCollection ApplyIds(this ThumbnailCard card, IdOptions options = null) => card?.Buttons?.ApplyIds(options);

        public static RecursiveIdCollection ApplyIds(this VideoCard card, IdOptions options = null) => card?.Buttons?.ApplyIds(options);

        public static RecursiveIdCollection ApplyIds(this IList<CardAction> buttons, IdOptions options = null)
        {
            if (buttons is null)
            {
                throw new ArgumentNullException(nameof(buttons));
            }

            var ids = new List<RecursiveIdCollection>();

            IdType.Card.ReplaceNullWithId(ref options);
            options.SetExistingId(IdType.Action);

            foreach (var button in buttons)
            {
                if ((button.Type == ActionTypes.MessageBack || button.Type == ActionTypes.PostBack) && button.Value != null)
                {
                    button.Value = button.Value.ToJObjectAndBackAsync(
                        value =>
                        {
                            ids.Add(value.ApplyIdsToPayload(options));
                            return Task.CompletedTask;
                        },
                        true).Result;
                }
            }

            return new RecursiveIdCollection(ids);
        }

        public static RecursiveIdCollection ApplyIdsToPayload(this JObject payload, IdOptions options = null)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (options is null)
            {
                options = new IdOptions(IdType.Action);
            }

            foreach (var type in Helper.GetEnumValues<IdType>())
            {
                if (options.Overwrite || payload.GetIdFromPayload(type) is null)
                {
                    var id = options.Get(type);

                    if (id is null)
                    {
                        if (type == IdType.Action)
                        {
                            // Only generate an ID for the action
                            id = IdType.Action.GenerateId();
                        }
                        else
                        {
                            // If any other ID's are null,
                            // don't apply them to the payload
                            continue;
                        }
                    }

                    payload[type.GetKey()] = id;
                }
            }

            return payload.GetIdsFromPayload();
        }

        public static RecursiveIdCollection GetIdsFromBatch(this IEnumerable<Activity> activities)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            var ids = new List<RecursiveIdCollection>();

            if (activities.Any())
            {
                foreach (var activity in activities)
                {
                    ids.Add(activity.GetIdsFromAttachments());
                }
            }

            return new RecursiveIdCollection(ids);
        }

        public static RecursiveIdCollection GetIdsFromAttachments(this Activity activity)
        {
            if (activity is null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var ids = new List<RecursiveIdCollection>();

            if (activity.Attachments?.Any() == true)
            {
                foreach (var attachment in activity.Attachments)
                {
                    ids.Add(attachment.GetIds() ?? new RecursiveIdCollection());
                }
            }

            return new RecursiveIdCollection(ids);
        }

        public static RecursiveIdCollection GetIds(this Attachment attachment)
        {
            if (attachment is null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            switch (attachment.ContentType)
            {
                case CardConstants.AdaptiveCardContentType:
                    return JObject.FromObject(attachment.Content).GetIdsFromAdaptiveCard();
                case AnimationCard.ContentType:
                    return (attachment.Content as AnimationCard)?.GetIds();
                case AudioCard.ContentType:
                    return (attachment.Content as AudioCard)?.GetIds();
                case HeroCard.ContentType:
                    return (attachment.Content as HeroCard)?.GetIds();
                case ReceiptCard.ContentType:
                    return (attachment.Content as ReceiptCard)?.GetIds();
                case SigninCard.ContentType:
                    return (attachment.Content as SigninCard)?.GetIds();
                case OAuthCard.ContentType:
                    return (attachment.Content as OAuthCard)?.GetIds();
                case ThumbnailCard.ContentType:
                    return (attachment.Content as ThumbnailCard)?.GetIds();
                case VideoCard.ContentType:
                    return (attachment.Content as VideoCard)?.GetIds();
            }

            return null;
        }

        public static RecursiveIdCollection GetIdsFromAdaptiveCard(this JObject adaptiveCard)
        {
            if (adaptiveCard is null)
            {
                throw new ArgumentNullException(nameof(adaptiveCard));
            }

            var ids = new List<RecursiveIdCollection>();

            // Iterate over all objects named "data" that aren't nested in another object named "data".
            // This effectively finds all the "payloads" of submit actions while accounting for situations
            // where a submit action's data itself contains a property with the name "data".
            foreach (var data in adaptiveCard.NonDataDescendants()
                .Select(token => token as JProperty)
                .Where(prop =>
                    prop?.Name.Equals(
                        CardConstants.KeyData,
                        StringComparison.OrdinalIgnoreCase) == true
                    && prop.Value?.Type == JTokenType.Object)
                .Select(prop => prop.Value as JObject))
            {
                ids.Add(data.GetIdsFromPayload());
            }

            return new RecursiveIdCollection(ids);
        }

        public static RecursiveIdCollection GetIds(this AnimationCard card) => card?.Buttons?.GetIds();

        public static RecursiveIdCollection GetIds(this AudioCard card) => card?.Buttons?.GetIds();

        public static RecursiveIdCollection GetIds(this HeroCard card) => card?.Buttons?.GetIds();

        public static RecursiveIdCollection GetIds(this OAuthCard card) => card?.Buttons?.GetIds();

        public static RecursiveIdCollection GetIds(this ReceiptCard card) => card?.Buttons?.GetIds();

        public static RecursiveIdCollection GetIds(this SigninCard card) => card?.Buttons?.GetIds();

        public static RecursiveIdCollection GetIds(this ThumbnailCard card) => card?.Buttons?.GetIds();

        public static RecursiveIdCollection GetIds(this VideoCard card) => card?.Buttons?.GetIds();

        public static RecursiveIdCollection GetIds(this IList<CardAction> buttons)
        {
            if (buttons is null)
            {
                throw new ArgumentNullException(nameof(buttons));
            }

            var ids = new List<RecursiveIdCollection>();

            foreach (var button in buttons)
            {
                if ((button.Type == ActionTypes.MessageBack || button.Type == ActionTypes.PostBack) && button.Value != null)
                {
                    button.Value.ToJObjectAndBackAsync(
                        value =>
                        {
                            ids.Add(value.GetIdsFromPayload());
                            return Task.CompletedTask;
                        },
                        true).Wait();
                }
            }

            return new RecursiveIdCollection(ids);
        }

        public static RecursiveIdCollection GetIdsFromPayload(this JObject payload)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var ids = new Dictionary<IdType, string>();

            foreach (var type in Helper.GetEnumValues<IdType>())
            {
                var id = payload.GetIdFromPayload(type);

                if (id != null)
                {
                    ids[type] = id;
                }
            }

            return new RecursiveIdCollection(ids);
        }

        public static string GetIdFromPayload(this JObject payload, IdType type = IdType.Card)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            return JsonConvert.SerializeObject(payload[type.GetKey()]);
        }

        public static IEnumerable<JToken> NonDataDescendants(this JContainer container)
        {
            return container.Descendants().Where(token =>
                !token.Ancestors().Any(ancestor =>
                    (ancestor as JProperty)?.Name.Equals(
                        CardConstants.KeyData,
                        StringComparison.OrdinalIgnoreCase) == true));
        }

        public static Activity ToAttachmentActivity(this Activity activity)
        {
            return new Activity
            {
                Id = activity.Id,
                AttachmentLayout = activity.AttachmentLayout,
                Attachments = activity.Attachments,
            };
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