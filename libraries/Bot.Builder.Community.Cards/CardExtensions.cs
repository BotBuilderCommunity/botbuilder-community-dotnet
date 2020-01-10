using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Cards.Nodes;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards
{
    public static class CardExtensions
    {
        public static void SeparateAttachments(this List<Activity> activities)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            // We need to iterate backwards because we're potentially changing the length of the list
            for (int i = activities.Count() - 1; i > -1; i--)
            {
                var activity = activities[i];
                var attachmentCount = activity.Attachments?.Count();
                var hasText = activity.Text != null;

                if (activity.AttachmentLayout == AttachmentLayoutTypes.List
                    && ((attachmentCount > 0 && hasText) || attachmentCount > 1))
                {
                    var separateActivities = new List<Activity>();
                    var js = new JsonSerializerSettings();
                    var json = JsonConvert.SerializeObject(activity, js);

                    if (hasText)
                    {
                        var textActivity = JsonConvert.DeserializeObject<Activity>(json, js);

                        textActivity.Attachments = null;
                        separateActivities.Add(textActivity);
                    }

                    foreach (var attachment in activity.Attachments)
                    {
                        var attachmentActivity = JsonConvert.DeserializeObject<Activity>(json, js);

                        attachmentActivity.Text = null;
                        attachmentActivity.Attachments = new List<Attachment> { attachment };
                        separateActivities.Add(attachmentActivity);
                    }

                    activities.RemoveAt(i);
                    activities.InsertRange(i, separateActivities);
                }
            }
        }

        public static void ApplyIdsToBatch(this IEnumerable<Activity> activities, IdOptions options = null)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            CardTree.ApplyIds(activities, NodeType.Batch, options);
        }

        public static void ApplyIdsToPayload(this JObject payload, IdOptions options = null)
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
        }

        public static IDictionary<IdType, ISet<string>> GetIdsFromBatch(this IEnumerable<Activity> activities)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            var dict = new Dictionary<IdType, ISet<string>>();

            CardTree.RecurseAsync(activities, (TypedId typedId) =>
            {
                dict.InitializeKey(typedId.Type, new HashSet<string>()).Add(typedId.Id);

                return Task.CompletedTask;
            }).Wait();

            return dict;
        }

        public static string GetIdFromPayload(this JObject payload, IdType type = IdType.Card)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            return payload[type.GetKey()] is JToken id ? JsonConvert.SerializeObject(id) : null;
        }

        public static IEnumerable<JToken> NonDataDescendants(this JContainer container)
        {
            return container.Descendants().Where(token =>
                !token.Ancestors().Any(ancestor =>
                    (ancestor as JProperty)?.Name.Equals(
                        CardConstants.KeyData,
                        StringComparison.OrdinalIgnoreCase) == true));
        }

        public static Activity ToStoredActivity(this Activity activity)
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