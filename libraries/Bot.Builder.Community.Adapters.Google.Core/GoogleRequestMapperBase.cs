using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bot.Builder.Community.Adapters.Google.Core.Attachments;
using Bot.Builder.Community.Adapters.Google.Core.Helpers;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Bot.Builder.Community.Adapters.Google.Core.Model.Request;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using BasicCard = Bot.Builder.Community.Adapters.Google.Core.Model.Response.BasicCard;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    public abstract class GoogleRequestMapperBase
    {
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
            activity.From = new ChannelAccount(request.GetUserIdFromUserStorage());
            activity.Id = Guid.NewGuid().ToString();
            activity.Timestamp = DateTime.UtcNow;
            activity.Locale = request.User.Locale;
            activity.ChannelData = request;

            return activity;
        }

        public static Activity MergeActivities(IList<Activity> activities)
        {
            return MappingHelper.MergeActivities(activities);
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

            activity.ConvertAttachmentContent();

            var basicCardItem = ProcessResponseItemAttachment<BasicCard>(GoogleAttachmentContentTypes.BasicCard, activity);
            if (basicCardItem != null)
                responseItems.Add(basicCardItem);

            var tableCardItem = ProcessResponseItemAttachment<TableCard>(GoogleAttachmentContentTypes.TableCard, activity);
            if (tableCardItem != null)
                responseItems.Add(tableCardItem);

            var mediaItem = ProcessResponseItemAttachment<MediaResponse>(GoogleAttachmentContentTypes.MediaResponse, activity);
            if (mediaItem != null)
                responseItems.Add(mediaItem);

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

        public static List<Suggestion> ConvertSuggestedActionsToSuggestionChips(Activity activity)
        {
            var suggestions = new List<Suggestion>();

            if (activity.SuggestedActions != null && activity.SuggestedActions.Actions != null && activity.SuggestedActions.Actions.Any())
            {
                foreach (var suggestion in activity.SuggestedActions.Actions)
                {
                    suggestions.Add(new Suggestion { Title = suggestion.Title });
                }
            }

            return suggestions;
        }
    }
}
