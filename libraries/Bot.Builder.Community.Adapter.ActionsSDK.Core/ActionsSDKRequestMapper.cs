using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Attachments;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Helpers;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems;
using Bot.Builder.Community.Adapters.Shared;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core
{
    public class ActionsSdkRequestMapper
    {
        private readonly ActionsSdkRequestMapperOptions _options;
        private readonly ILogger _logger;

        public ActionsSdkRequestMapper(ActionsSdkRequestMapperOptions options = null, ILogger logger = null)
        {
            _options = options ?? new ActionsSdkRequestMapperOptions();
            _logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(ActionsSdkRequest request)
        {
            var activity = new Activity();

            var actionIntent = request.Intent.Name;

            // Handle system intents
            switch (actionIntent?.ToLowerInvariant())
            {
                case "actions.intent.media_status_finished":
                case "actions.intent.media_status_paused":
                case "actions.intent.media_status_stopped":
                case "actions.intent.media_status_failed":
                case "actions.intent.play_game":
                    activity.Type = ActivityTypes.Event;
                    activity = SetGeneralActivityProperties(activity, request);
                    activity.Name = actionIntent;
                    activity.Value = request;
                    return activity;
                case "actions.intent.cancel":
                    activity = Activity.CreateEndOfConversationActivity() as Activity;
                    activity = SetGeneralActivityProperties(activity, request);
                    return activity;
            }

            // Handle special case handlers that are part of our model
            switch (request.Handler.Name)
            {
                case "accountlinkingcompleted":
                    activity.Type = ActivityTypes.Event;
                    activity = SetGeneralActivityProperties(activity, request);
                    activity.Name = "AccountLinking";
                    activity.Value = "Completed";
                    return activity;
                case "accountlinkingcancelled":
                    activity.Type = ActivityTypes.Event;
                    activity = SetGeneralActivityProperties(activity, request);
                    activity.Name = "AccountLinking";
                    activity.Value = "Cancelled";
                    return activity;
                case "accountlinkingerror":
                    activity.Type = ActivityTypes.Event;
                    activity = SetGeneralActivityProperties(activity, request);
                    activity.Name = "AccountLinking";
                    activity.Value = "Error";
                    return activity;
            }

            // If not handled system intent / special case handler, proceed with
            // handling user query, starting with stripping the invocation name
            var text = StripInvocation(request.Intent.Query, _options.ActionInvocationName);

            // If the query is empty at this point, we can assume that a user
            // has invoked the action without indicating an intent and we send
            // a conversation update
            if (string.IsNullOrEmpty(text))
            {
                activity.Type = ActivityTypes.ConversationUpdate;
                activity = SetGeneralActivityProperties(activity, request);
                activity.MembersAdded = new List<ChannelAccount>() { new ChannelAccount() { Id = activity.From?.Id ?? "anonymous" } };
                return activity;
            }

            // We have text from a user query after we have stripped the invocation name / standard text
            // so we send this as a message activity
            activity.Type = ActivityTypes.Message;
            activity = SetGeneralActivityProperties(activity, request);
            activity.Text = text;
            return activity;
        }

        public Activity SetGeneralActivityProperties(Activity activity, ActionsSdkRequest request)
        {
            activity.DeliveryMode = DeliveryModes.ExpectReplies;
            activity.ChannelId = _options.ChannelId;
            activity.ServiceUrl = _options.ServiceUrl;
            activity.Recipient = new ChannelAccount("", "action");
            activity.Conversation = new ConversationAccount(false, id: $"{request.Session.Id}");
            activity.From = new ChannelAccount($"{request.Session.Id}_user");
            activity.Id = Guid.NewGuid().ToString();
            activity.Timestamp = DateTime.UtcNow;
            activity.Locale = request.User.Locale;
            activity.ChannelData = request;

            return activity;
        }

        public ActionsSdkResponse ActivityToResponse(Activity activity, ActionsSdkRequest request)
        {
            var response = new ActionsSdkResponse();

            // Send default empty response if no activity or invalid activity type sent
            if (activity == null || activity.Type != ActivityTypes.Message)
            {
                response.Scene = new Scene()
                {
                    Next = new NextScene() { Name = "ListPrompt" }
                };
                return response;
            }

            response.Session = new Session()
            {
                Id = activity.Conversation.Id,
                Params = null,
                LanguageCode = string.Empty
            };

            if (activity.Attachments.Any(a => a.ContentType == SigninCard.ContentType))
            {
                response.Scene = new Scene()
                {
                    Next = new NextScene()
                    {
                        Name = "AccountLinkingCheck"
                    }
                };

                return response;
            }

            response.Prompt = new Prompt
            {
                FirstSimple = new Simple
                {
                    Speech = activity.Speak ?? activity.Text, Text = activity.Text
                }
            };

            var contentItem = GetContentItemsFromActivityAttachments(activity).FirstOrDefault();

            switch (contentItem)
            {
                case ListContentItem listContentItem:
                    response.Prompt.Override = true;
                    response.Prompt.Content = new InternalListContentItem()
                    {
                        InternalList = new InternalList()
                        {
                            Title = listContentItem.Title,
                            Items = listContentItem.Items.Select(i => new InternalListItem() { Key = i.Key }).ToList(),
                            Subtitle = listContentItem.Subtitle
                        }
                    };
                    response.Session.TypeOverrides = new List<TypeOverride>()
                    {
                        new TypeOverride()
                        {
                            TypeOverrideMode = TypeOverrideMode.TYPE_REPLACE,
                            Name = "prompt_option",
                            Synonym = new SynonymType()
                            {
                                Entries = listContentItem.Items.Select(i => new Entry() { Name = i.Key, Display = i.Item, Synonyms = i.Synonyms }).ToList()
                            }
                        }
                    };
                break;
                case CollectionContentItem collectionContentItem:
                    response.Prompt.Content = new InternalCollectionContentItem()
                    {
                        Collection = new Collection()
                        {
                            Title = collectionContentItem.Title,
                            Items = collectionContentItem.Items.Select(i => new InternalCollectionItem() { Key = i.Key }).ToList(),
                            Subtitle = collectionContentItem.Subtitle,
                            ImageFill = collectionContentItem.ImageFill
                        }
                    };
                    response.Session.TypeOverrides = new List<TypeOverride>()
                    {
                        new TypeOverride()
                        {
                            TypeOverrideMode = TypeOverrideMode.TYPE_REPLACE,
                            Name = "prompt_option",
                            Synonym = new SynonymType()
                            {
                                Entries = collectionContentItem.Items.Select(i => new Entry() { Name = i.Key, Display = i.Item, Synonyms = i.Synonyms  }).ToList()
                            }
                        }
                    };
                    break;
                default:
                    response.Prompt.Content = contentItem;
                    break;
        }

            // ensure InputHint is set as required for response
            if (activity.InputHint == null || activity.InputHint == InputHints.AcceptingInput)
            {
                activity.InputHint =
                    _options.ShouldEndSessionByDefault ? InputHints.IgnoringInput : InputHints.ExpectingInput;
            }

            // check if we should be listening for more input from the user
            switch (activity.InputHint)
            {
                case InputHints.IgnoringInput:
                    response.Scene = new Scene()
                    {
                        Next = new NextScene() { Name = "actions.scene.END_CONVERSATION" }
                    };
                    break;
                case InputHints.ExpectingInput:
                    response.Scene = new Scene()
                        {
                            Next = new NextScene()
                            {
                                Name = "ListPrompt"
                            }
                        };
                    response.Prompt.Suggestions = ConvertImAndMessageBackSuggestedActionsToSuggestionChips(activity);
                    break;
            }

            return response;
        }

        public static Activity MergeActivities(IList<Activity> activities)
        {
            return ActivityMappingHelper.MergeActivities(activities);
        }

        public ContentItem ProcessContentItemAttachment<T>(string contentType, Activity activity) where T : ContentItem
        {
            return activity?.Attachments?
                .Where(a => a.ContentType == contentType)
                .Select(a => (T)a.Content).FirstOrDefault();
        }

        public List<ContentItem> GetContentItemsFromActivityAttachments(Activity activity)
        {
            var contentItems = new List<ContentItem>();

            activity.ConvertAttachmentContent();

            var bfCard = activity.Attachments?.FirstOrDefault(a => a.ContentType == HeroCard.ContentType);

            if (bfCard != null)
            {
                contentItems.Add(CreateActionsSdkCardFromHeroCard((HeroCard)bfCard.Content));
            }

            var cardItem = ProcessContentItemAttachment<CardContentItem>(ActionsSdkAttachmentContentTypes.Card, activity);
            if (cardItem != null)
                contentItems.Add(cardItem);

            var tableItem = ProcessContentItemAttachment<TableContentItem>(ActionsSdkAttachmentContentTypes.Table, activity);
            if (tableItem != null)
                contentItems.Add(tableItem);

            var mediaItem = ProcessContentItemAttachment<MediaContentItem>(ActionsSdkAttachmentContentTypes.Media, activity);
            if (mediaItem != null)
                contentItems.Add(mediaItem);

            var collectionItem = ProcessContentItemAttachment<CollectionContentItem>(ActionsSdkAttachmentContentTypes.Collection, activity);
            if (collectionItem != null)
                contentItems.Add(collectionItem);

            var listItem = ProcessContentItemAttachment<ListContentItem>(ActionsSdkAttachmentContentTypes.List, activity);
            if (listItem != null)
                contentItems.Add(listItem);

            return contentItems;
        }

        public string StripInvocation(string query, string invocationName)
        {
            if (!string.IsNullOrEmpty(query) && !string.IsNullOrEmpty(invocationName))
            {
                if (query.Contains(invocationName))
                {
                    var newStartPosition = query.IndexOf(invocationName, StringComparison.OrdinalIgnoreCase);
                    query = query.Substring(newStartPosition + invocationName.Length);
                }
            }

            return query?.Trim();
        }

        public static List<Suggestion> ConvertImAndMessageBackSuggestedActionsToSuggestionChips(Activity activity)
        {
            List<Suggestion> suggestions = null;

            if (activity.SuggestedActions != null && activity.SuggestedActions.Actions != null && activity.SuggestedActions.Actions.Any())
            {
                suggestions = new List<Suggestion>();

                foreach (var suggestion in activity.SuggestedActions.Actions)
                {
                    if (suggestion.Type == ActionTypes.ImBack || suggestion.Type == ActionTypes.MessageBack)
                    {
                        suggestions.Add(new Suggestion { Title = suggestion.Title });
                    }
                }
            }

            return suggestions;
        }

        private CardContentItem CreateActionsSdkCardFromHeroCard(HeroCard heroCard)
        {
            var card = ContentItemFactory.CreateCard(heroCard.Title, heroCard.Subtitle);

            if (heroCard.Images != null && heroCard.Images.Any())
            {
                card.Card.Image = new Image()
                {
                    Url = heroCard.Images?.FirstOrDefault()?.Url,
                    Alt = heroCard.Images?.FirstOrDefault()?.Alt,
                };
            }

            return card;
        }
    }
}
