﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapter.ActionsSDK.Core.Attachments;
using Bot.Builder.Community.Adapter.ActionsSDK.Core.Helpers;
using Bot.Builder.Community.Adapter.ActionsSDK.Core.Model;
using Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core
{
    public class ActionsSdkRequestMapper
    {
        public ActionsSdkRequestMapper(ActionsSdkRequestMapperOptions options = null, ILogger logger = null)
        {
            Options = options ?? new ActionsSdkRequestMapperOptions();
            Logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(ActionsSdkRequest request)
        {
            var activity = new Activity();

            var actionIntent = request.Intent.Name;

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
                case "actions.intent.sign_in":
                    //activity.Type = ActivityTypes.Event;
                    //activity = SetGeneralActivityProperties(activity, request);
                    //activity.Name = actionIntent;
                    //var signinStatusArgument = request.Inputs.First()?.Arguments?.Where(a => a.Name == "SIGN_IN").FirstOrDefault();
                    //var argumentExtension = signinStatusArgument?.Extension;
                    //activity.Value = argumentExtension?["status"];
                    //return activity;
                case "actions.intent.cancel":
                    activity = Activity.CreateEndOfConversationActivity() as Activity;
                    activity = SetGeneralActivityProperties(activity, request);
                    return activity;
            }

            var text = StripInvocation(request.Intent.Query, Options.ActionInvocationName);

            if (string.IsNullOrEmpty(text))
            {
                activity.Type = ActivityTypes.ConversationUpdate;
                activity = SetGeneralActivityProperties(activity, request);
                activity.MembersAdded = new List<ChannelAccount>() { new ChannelAccount() { Id = activity.From?.Id ?? "anonymous" } };
                return activity;
            }

            activity.Type = ActivityTypes.Message;
            activity = SetGeneralActivityProperties(activity, request);
            activity.Text = text;
            return activity;
        }

        public Activity SetGeneralActivityProperties(Activity activity, ActionsSdkRequest request)
        {
            activity.DeliveryMode = DeliveryModes.ExpectReplies;
            activity.ChannelId = Options.ChannelId;
            activity.ServiceUrl = Options.ServiceUrl;
            activity.Recipient = new ChannelAccount("", "action");
            activity.Conversation = new ConversationAccount(false, id: $"{request.Session.Id}");
            activity.From = null;
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
                Params = new Dictionary<string, JObject>(),
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
                    Options.ShouldEndSessionByDefault ? InputHints.IgnoringInput : InputHints.ExpectingInput;
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

        public ActionsSdkRequestMapperOptions Options { get; set; }
        public ILogger Logger;

        public static Activity MergeActivities(IList<Activity> activities)
        {
            return MappingHelper.MergeActivities(activities);
        }

        public ContentItem ProcessContentItemAttachment<T>(string contentType, Activity activity) where T : ContentItem
        {
            return activity.Attachments?
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
                invocationName = invocationName.ToLowerInvariant();
                query = query.ToLowerInvariant();

                if (query.Contains(invocationName))
                {
                    var newStartPosition = query.IndexOf(invocationName, StringComparison.Ordinal);
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
                    Url = heroCard.Images.FirstOrDefault().Url,
                    Alt = heroCard.Images.FirstOrDefault().Alt,
                };
            }

            return card;
        }
    }
}