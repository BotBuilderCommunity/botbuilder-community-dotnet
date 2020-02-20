using System.Collections.Generic;
using System.Linq;
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
                return;
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

        /// <summary>
        /// This will convert Adaptive Cards to JObject instances to work around this issue:
        /// https://github.com/microsoft/AdaptiveCards/issues/2148.
        /// </summary>
        /// <param name="activities">A batch of activities.</param>
        public static void ConvertAdaptiveCards(this IEnumerable<Activity> activities)
        {
            if (activities is null)
            {
                return;
            }

            CardTree.Recurse(activities, (Attachment attachment) =>
            {
                if (attachment.ContentType == CardConstants.AdaptiveCardContentType)
                {
                    attachment.Content = attachment.Content.ToJObject();
                }
            });
        }

        public static void ApplyIdsToBatch(this IEnumerable<Activity> activities, PayloadIdOptions options = null)
        {
            if (activities is null)
            {
                return;
            }

            CardTree.ApplyIds(activities, options);
        }

        public static IDictionary<string, ISet<string>> GetIdsFromBatch(this IEnumerable<Activity> activities)
        {
            if (activities is null)
            {
                return null;
            }

            var dict = new Dictionary<string, ISet<string>>();

            CardTree.Recurse(activities, (PayloadId payloadId) =>
            {
                dict.InitializeKey(payloadId.Type, new HashSet<string>()).Add(payloadId.Id);
            });

            return dict;
        }

        public static void ApplyIdsToPayload(this JObject payload, PayloadIdOptions options = null)
        {
            if (payload is null)
            {
                return;
            }

            if (options is null)
            {
                options = new PayloadIdOptions(PayloadIdTypes.Action);
            }

            foreach (var kvp in options.GetIds())
            {
                var type = kvp.Key;

                if (options.Overwrite || payload.GetIdFromPayload(type) is null)
                {
                    var id = kvp.Value;

                    if (id is null)
                    {
                        if (type == PayloadIdTypes.Action)
                        {
                            // Only generate an ID for the action
                            id = PayloadIdTypes.GenerateId(PayloadIdTypes.Action);
                        }
                        else
                        {
                            // If any other ID's are null,
                            // don't apply them to the payload
                            continue;
                        }
                    }

                    payload[PayloadIdTypes.GetKey(type)] = id;
                }
            }
        }

        public static string GetIdFromPayload(this JObject payload, string type = PayloadIdTypes.Card) =>
            payload?.GetValueCI(PayloadIdTypes.GetKey(type)) is JToken id ? id.ToString() : null;

        public static void AdaptOutgoingCardActions(this List<Activity> activities, string channelId = null)
        {
            if (activities is null)
            {
                return;
            }

            foreach (var activity in activities)
            {
                channelId = channelId ?? activity.ChannelId;

                CardTree.Recurse(activity, (CardAction action) =>
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
                });
            }
        }

        /// <summary>
        /// This will return null if the incoming activity is not from a button.
        /// The returned value is guaranteed to not be a string.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <returns>A button's payload if valid, null otherwise.</returns>
        public static JObject GetIncomingPayload(this ITurnContext turnContext)
        {
            if (turnContext is null)
            {
                return null;
            }

            var turnState = turnContext.TurnState.Get<CardManagerTurnState>();

            if (turnState is null)
            {
                turnContext.TurnState.Set(turnState = new CardManagerTurnState());
            }

            if (turnState.CheckedForIncomingButtonPayload)
            {
                return turnState.IncomingButtonPayload;
            }

            var activity = turnContext.Activity;

            if (activity is null)
            {
                return null;
            }

            var text = activity.Text;
            var parsedText = text.TryParseJObject();
            var value = activity.Value.ToJObject(true);
            var channelData = activity.ChannelData.ToJObject(true); // Channel data will have been serialized into a string in Kik
            var entities = activity.Entities;

            turnState.IncomingButtonPayload = value;
            turnState.CheckedForIncomingButtonPayload = true;

            // Many channels have button responses that are hard to distinguish from user-entered text.
            // A common theme is that button responses often have a property in channel data that isn't
            // present in a typed user-to-bot message, so this local function helps check for that.
            void CheckForChannelDataProperty(string propName, JObject newResult = null)
            {
                if (channelData?.GetValueCI(propName) != null)
                {
                    turnState.IncomingButtonPayload = newResult ?? parsedText;
                }
            }

            switch (activity.ChannelId)
            {
                case Channels.Cortana:

                    // In Cortana, the only defining characteristic of button responses
                    // is that they won't have an "Intent" entity.
                    // This if statement uses `!= true` because we're interpreting a null entities list
                    // as confirmation of a missing "Intent" entity.
                    if (entities?.Any(entity => entity.Type.EqualsCI(CardConstants.TypeIntent)) != true)
                    {
                        turnState.IncomingButtonPayload = parsedText;
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
                    // This if statement uses `== false` because if the channel data is null or has no text property
                    // then we're interpreting that to mean that this is not a button response.
                    if (channelData?.GetValueCI(CardConstants.KeyText)?.ToString().EqualsCI(text) == false)
                    {
                        turnState.IncomingButtonPayload = parsedText;
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

            return turnState.IncomingButtonPayload;
        }
    }
}
