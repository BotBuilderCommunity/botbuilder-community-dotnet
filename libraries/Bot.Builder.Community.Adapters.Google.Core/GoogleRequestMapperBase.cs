﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Google.Core.Attachments;
using Bot.Builder.Community.Adapters.Google.Core.Helpers;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Bot.Builder.Community.Adapters.Google.Core.Model.Request;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Bot.Builder.Community.Adapters.Shared;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using BasicCard = Bot.Builder.Community.Adapters.Google.Core.Model.Response.BasicCard;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    public abstract class GoogleRequestMapperBase
    {
        protected static readonly AttachmentConverter _attachmentConverter = DefaultGoogleAttachmentConverter.CreateDefault();

        public GoogleRequestMapperOptions Options { get; set; }
        public ILogger Logger;

        public GoogleRequestMapperBase(GoogleRequestMapperOptions options = null, ILogger logger = null)
        {
            Options = options ?? new GoogleRequestMapperOptions();
            Logger = logger ?? NullLogger.Instance;
        }

        public Activity SetGeneralActivityProperties(Activity activity, ConversationRequest request)
        {
            activity.DeliveryMode = DeliveryModes.ExpectReplies;
            activity.ChannelId = Options.ChannelId;
            activity.ServiceUrl = Options.ServiceUrl;
            activity.Recipient = new ChannelAccount("", "action");
            activity.Conversation = new ConversationAccount(false, id: $"{request.Conversation.ConversationId}");
            activity.From = new ChannelAccount(GetOrSetUserId(request));
            activity.Id = Guid.NewGuid().ToString();
            activity.Timestamp = DateTime.UtcNow;
            activity.Locale = request.User.Locale;
            activity.ChannelData = request;

            return activity;
        }

        public static Activity MergeActivities(IList<Activity> activities)
        {
            return ActivityMappingHelper.MergeActivities(activities);
        }

        public ProcessHelperIntentAttachmentsResult ProcessHelperIntentAttachments(Activity activity)
        {
            if (activity?.Attachments != null
                && activity.Attachments.FirstOrDefault(a => a.ContentType == SigninCard.ContentType) != null)
            {
                return new ProcessHelperIntentAttachmentsResult()
                {
                    Intent = new SigninIntent(),
                    AllowAdditionalInputPrompt = false
                };
            }

            if (activity?.Attachments?.FirstOrDefault(a =>
                    a.ContentType == GoogleAttachmentContentTypes.ConfirmationIntent) != null)
            {
                return new ProcessHelperIntentAttachmentsResult()
                {
                    Intent = ProcessSystemIntentAttachment<ConfirmationIntent>(
                        GoogleAttachmentContentTypes.ConfirmationIntent,
                        activity),
                    AllowAdditionalInputPrompt = false
                };
            }

            if (activity?.Attachments?.FirstOrDefault(a =>
                    a.ContentType == GoogleAttachmentContentTypes.DateTimeIntent) != null)
            {
                return new ProcessHelperIntentAttachmentsResult()
                {
                    Intent = ProcessSystemIntentAttachment<DateTimeIntent>(
                        GoogleAttachmentContentTypes.DateTimeIntent,
                        activity),
                    AllowAdditionalInputPrompt = false
                };
            }

            if (activity?.Attachments?.FirstOrDefault(a =>
                    a.ContentType == GoogleAttachmentContentTypes.PermissionsIntent) != null)
            {
                return new ProcessHelperIntentAttachmentsResult()
                {
                    Intent = ProcessSystemIntentAttachment<PermissionsIntent>(
                        GoogleAttachmentContentTypes.PermissionsIntent,
                        activity),
                    AllowAdditionalInputPrompt = false
                };
            }

            if (activity?.Attachments?.FirstOrDefault(a =>
                    a.ContentType == GoogleAttachmentContentTypes.PlaceLocationIntent) != null)
            {
                return new ProcessHelperIntentAttachmentsResult()
                {
                    Intent = ProcessSystemIntentAttachment<PlaceLocationIntent>(
                        GoogleAttachmentContentTypes.PlaceLocationIntent,
                        activity),
                    AllowAdditionalInputPrompt = false
                };
            }

            if (activity?.Attachments?.FirstOrDefault(a =>
                    a.ContentType == GoogleAttachmentContentTypes.SigninIntent) != null)
            {
                return new ProcessHelperIntentAttachmentsResult()
                {
                    Intent = ProcessSystemIntentAttachment<SigninIntent>(
                        GoogleAttachmentContentTypes.SigninIntent,
                        activity),
                    AllowAdditionalInputPrompt = false
                };
            }

            if (activity?.Attachments?.FirstOrDefault(a =>
                    a.ContentType == GoogleAttachmentContentTypes.CarouselIntent) != null)
            {
                return new ProcessHelperIntentAttachmentsResult()
                {
                    Intent = ProcessSystemIntentAttachment<CarouselIntent>(
                        GoogleAttachmentContentTypes.CarouselIntent,
                        activity),
                    AllowAdditionalInputPrompt = true
                };
            }

            if (activity?.Attachments?.FirstOrDefault(a =>
                    a.ContentType == GoogleAttachmentContentTypes.ListIntent) != null)
            {
                return new ProcessHelperIntentAttachmentsResult()
                {
                    Intent = ProcessSystemIntentAttachment<ListIntent>(
                        GoogleAttachmentContentTypes.ListIntent,
                        activity),
                    AllowAdditionalInputPrompt = true
                };
            }

            if (activity?.Attachments?.FirstOrDefault(a =>
                    a.ContentType == GoogleAttachmentContentTypes.NewSurfaceIntent) != null)
            {
                return new ProcessHelperIntentAttachmentsResult()
                {
                    Intent = ProcessSystemIntentAttachment<NewSurfaceIntent>(
                        GoogleAttachmentContentTypes.NewSurfaceIntent,
                        activity),
                    AllowAdditionalInputPrompt = false
                };
            }

            return new ProcessHelperIntentAttachmentsResult()
            {
                Intent = null
            };
        }

        public ResponseItem ProcessResponseItemAttachment<T>(string contentType, Activity activity) where T : ResponseItem
        {
            return activity.Attachments?
                .Where(a => a.ContentType == contentType)
                .Select(a => (T)a.Content).FirstOrDefault();
        }

        public SystemIntent ProcessSystemIntentAttachment<T>(string contentType, Activity activity) where T : SystemIntent
        {
            return activity.Attachments?
                .Where(a => a.ContentType == contentType)
                .Select(a => (T)a.Content).FirstOrDefault();
        }

        public List<ResponseItem> GetResponseItemsFromActivityAttachments(Activity activity)
        {
            var responseItems = new List<ResponseItem>();

            _attachmentConverter.ConvertAttachments(activity);

            var bfCard = activity.Attachments?.FirstOrDefault(a => a.ContentType == HeroCard.ContentType);

            if (bfCard != null)
            {
                switch (bfCard.Content)
                {
                    case HeroCard heroCard:
                        responseItems.Add(CreateGoogleCardFromHeroCard(heroCard));
                        break;
                }
            }
            else
            {
                var basicCardItem = ProcessResponseItemAttachment<BasicCard>(GoogleAttachmentContentTypes.BasicCard, activity);
                if (basicCardItem != null)
                    responseItems.Add(basicCardItem);

                var tableCardItem = ProcessResponseItemAttachment<TableCard>(GoogleAttachmentContentTypes.TableCard, activity);
                if (tableCardItem != null)
                    responseItems.Add(tableCardItem);

                var mediaItem = ProcessResponseItemAttachment<MediaResponse>(GoogleAttachmentContentTypes.MediaResponse, activity);
                if (mediaItem != null)
                    responseItems.Add(mediaItem);

                var browsingCarousel = ProcessResponseItemAttachment<BrowsingCarousel>(GoogleAttachmentContentTypes.BrowsingCarousel, activity);
                if (browsingCarousel != null)
                    responseItems.Add(browsingCarousel);
            }

            return responseItems;
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

            return query;
        }

        public static List<Suggestion> ConvertIMAndMessageBackSuggestedActionsToSuggestionChips(Activity activity)
        {
            var suggestions = new List<Suggestion>();

            if (activity.SuggestedActions != null && activity.SuggestedActions.Actions != null && activity.SuggestedActions.Actions.Any())
            {
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

        public static LinkOutSuggestion GetLinkOutSuggestionFromActivity(Activity activity)
        {
            var openUrlSuggestedAction = activity.SuggestedActions?.Actions?.Where(a => a.Type == ActionTypes.OpenUrl).FirstOrDefault();

            if(openUrlSuggestedAction == null)
            {
                return null;
            }

            return new LinkOutSuggestion()
            {
                DestinationName = openUrlSuggestedAction.Title,
                OpenUrlAction = new OpenUrlAction()
                {
                    Url = openUrlSuggestedAction.Value?.ToString(),
                    UrlTypeHint = UrlTypeHint.URL_TYPE_HINT_UNSPECIFIED
                }
            };
        }

        public static string GetOrSetUserId(ConversationRequest request)
        {
            if (request.User.UserVerificationStatus != "VERIFIED")
            {
                request.User.UserStorage = request.Conversation.ConversationId;
                return request.Conversation.ConversationId;
            }

            if (!string.IsNullOrEmpty(request.User.UserStorage?.ToString()))
            {
                Guid.TryParse(request.User.UserStorage.ToString(), out Guid currentUserId);

                if (currentUserId != Guid.Empty)
                {
                    return currentUserId.ToString();
                }
            }
            
            var newUserId = Guid.NewGuid();
            request.User.UserStorage = newUserId;
            return newUserId.ToString();
        }

        private BasicCard CreateGoogleCardFromHeroCard(HeroCard heroCard)
        {
            var imageUrl = heroCard.Images?.FirstOrDefault()?.Url;
            var buttons = new List<Button>();

            var heroCardButtons = heroCard.Buttons
                .Where(b => b.Type == ActionTypes.OpenUrl && b.Value is string buttonValue && buttonValue.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (heroCardButtons?.FirstOrDefault() is CardAction button)
            {
                if (heroCardButtons.Count() > 1)
                {
                    Logger.LogWarning("Only one 'button' is supported on Google basic card, using first button");
                }

                buttons.Add(new Button()
                {
                    Title = button.Title,
                    OpenUrlAction = new OpenUrlAction() { Url = button.Value as string }
                });
            }

            return GoogleCardFactory.CreateBasicCard(heroCard.Title, heroCard.Subtitle, heroCard.Text,  buttons, imageUrl != null ? new Image { Url = imageUrl } : null);
        }
    }
}
