using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Cards.Nodes;
using Microsoft.Bot.Connector;
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

        public static void AdaptCardActions(this List<Activity> activities, string channelId)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            CardTree.RecurseAsync(activities, (CardAction action) =>
            {
                var text = action.Text;
                var value = action.Value;
                var type = action.Type;

                void EnsureText()
                {
                    if (text == null && value != null)
                    {
                        action.Text = JsonConvert.SerializeObject(value);
                    }
                }

                void EnsureValue()
                {
                    if (value == null && text != null)
                    {
                        action.Value = text;
                    }
                }

                void EnsureStringValue()
                {
                    if (!(value is string))
                    {
                        if (value != null)
                        {
                            action.Value = JsonConvert.SerializeObject(value);
                        }
                        else if (text != null)
                        {
                            action.Value = text;
                        }
                    }
                }

                void EnsureObjectValue()
                {
                    if (value is string stringValue && stringValue.TryParseJObject() is JObject parsedValue)
                    {
                        action.Value = parsedValue;
                    }
                }

                if (type == ActionTypes.MessageBack)
                {
                    switch (channelId)
                    {
                        case Channels.Cortana:
                        case Channels.Skype:
                            // MessageBack does not work on these channels
                            action.Type = ActionTypes.PostBack;
                            break;

                        case Channels.Directline:
                        case Channels.Emulator:
                        case Channels.Line:
                        case Channels.Webchat:
                            EnsureValue();
                            break;

                        case Channels.Email:
                        case Channels.Slack:
                        case Channels.Telegram:
                            EnsureText();
                            break;

                        case Channels.Facebook:
                            EnsureStringValue();
                            break;

                        case Channels.Msteams:
                            EnsureObjectValue();
                            break;
                    }
                }

                // Using if instead of else-if so this block can be executed in addition to the previous one
                if (type == ActionTypes.PostBack)
                {
                    switch (channelId)
                    {
                        case Channels.Cortana:
                        case Channels.Facebook:
                        case Channels.Slack:
                        case Channels.Telegram:
                            EnsureStringValue();
                            break;

                        case Channels.Directline:
                        case Channels.Email:
                        case Channels.Emulator:
                        case Channels.Line:
                        case Channels.Skype:
                        case Channels.Webchat:
                            EnsureValue();
                            break;

                        case Channels.Msteams:
                            EnsureObjectValue();
                            break;
                    }
                }

                if (type == ActionTypes.ImBack)
                {
                    switch (channelId)
                    {
                        case Channels.Cortana:
                        case Channels.Directline:
                        case Channels.Emulator:
                        case Channels.Facebook:
                        case Channels.Msteams:
                        case Channels.Skype:
                        case Channels.Slack:
                        case Channels.Telegram:
                        case Channels.Webchat:
                            EnsureStringValue();
                            break;

                        case Channels.Email:
                        case Channels.Line:
                            EnsureValue();
                            break;
                    }
                }

                return Task.CompletedTask;
            }).Wait();
        }

        public static void ApplyIdsToBatch(this IEnumerable<Activity> activities, IdOptions options = null)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            CardTree.ApplyIds(activities, options);
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

            return payload[type.GetKey()] is JToken id ? id.ToString() : null;
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