using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Cards.Management.Tree;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public static class ManagementExtensions
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

        public static void ApplyIdsToBatch(this IEnumerable<Activity> activities, PayloadIdOptions options = null)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            CardTree.ApplyIds(activities, options);
        }

        public static IDictionary<PayloadIdType, ISet<string>> GetIdsFromBatch(this IEnumerable<Activity> activities)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            var dict = new Dictionary<PayloadIdType, ISet<string>>();

            CardTree.RecurseAsync(activities, (PayloadId payloadId) =>
            {
                dict.InitializeKey(payloadId.Type, new HashSet<string>()).Add(payloadId.Id);

                return Task.CompletedTask;
            }).Wait();

            return dict;
        }

        public static void AdaptOutgoingCardActions(this List<Activity> activities, string channelId = null)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            foreach (var activity in activities)
            {
                channelId = channelId ?? activity.ChannelId;

                CardTree.RecurseAsync(activity, (CardAction action) =>
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
        }

        /// <summary>
        /// This will return null if the incoming activity is not from a button.
        /// The returned value is guaranteed to not be a string.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <returns>A button's payload if valid, null otherwise.</returns>
        public static object GetIncomingButtonPayload(this ITurnContext turnContext)
        {
            if (!(turnContext?.Activity is Activity activity))
            {
                return null;
            }

            var text = activity.Text;
            var parsedText = text.TryParseJObject();
            var value = activity.Value.ToJObject(true);
            var channelData = activity.ChannelData.ToJObject(true); // Channel data will have been serialized into a string in Kik
            var entities = activity.Entities;
            var result = value;

            // Many channels have button responses that are hard to distinguish from user-entered text.
            // A common theme is that button responses often have a property in channel data that isn't
            // present in a typed user-to-bot message, so this local function helps check for that.
            void CheckForChannelDataProperty(string propName, JObject newResult = null)
            {
                if (channelData?.GetValueCI(propName) != null)
                {
                    result = newResult ?? parsedText;
                }
            }

            switch (activity.ChannelId)
            {
                case Channels.Cortana:

                    // In Cortana, the only defining characteristic of button responses
                    // is that they won't have an "Intent" entity.
                    if (entities?.Any(entity => entity.Type.EqualsCI(CardConstants.TypeIntent)) != true)
                    {
                        result = parsedText;
                    }

                    break;

                case Channels.Directline:
                case Channels.Emulator:
                case Channels.Webchat:

                    // In Direct Line / Web Chat, button responses can be recognized by a property of channel data.
                    CheckForChannelDataProperty(CardConstants.KeyPostBack, value);
                    CheckForChannelDataProperty(CardConstants.KeyMessageBack, value);

                    break;

                case Channels.Kik:

                    // In Kik, button responses can be recognized by a property of channel data.
                    // Note that this condition will be true because metadata will not be a C# null,
                    // even though it's a null JValue.
                    CheckForChannelDataProperty(CardConstants.KeyMetadata);

                    break;

                case Channels.Line:

                    // In LINE, button responses can be recognized by a property of channel data.
                    CheckForChannelDataProperty(CardConstants.KeyLinePostback);

                    break;

                case Channels.Skype:

                    // In Skype, the only defining characteristic of button responses
                    // is that the channel data text does not match the activity text.
                    if (channelData?.GetValueCI(CardConstants.KeyText)?.ToString().EqualsCI(text) == false)
                    {
                        result = parsedText;
                    }

                    break;

                case Channels.Slack:

                    // In Slack, button responses can be recognized by a property of channel data.
                    CheckForChannelDataProperty(CardConstants.KeyPayload);

                    break;

                case Channels.Telegram:

                    // In Telegram, button responses can be recognized by a property of channel data.
                    CheckForChannelDataProperty(CardConstants.KeyCallbackQuery);

                    break;
            }

            // Teams and Facebook values don't need to be adapted

            return result;
        }
    }
}
