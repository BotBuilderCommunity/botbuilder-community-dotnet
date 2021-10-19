using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Attachments;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core
{
    public class GoogleBusinessMessagingRequestMapper
    {
        private readonly GoogleBusinessMessagingRequestMapperOptions _options;
        private readonly ILogger _logger;

        protected static readonly AttachmentConverter AttachmentConverter = DefaultGoogleAttachmentConverter.CreateDefault();

        public GoogleBusinessMessagingRequestMapper(GoogleBusinessMessagingRequestMapperOptions options = null, ILogger logger = null)
        {
            _options = options ?? new GoogleBusinessMessagingRequestMapperOptions();
            _logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(GoogleBusinessRequest request)
        {
            if (request.Message != null || request.SuggestionResponse?.Type == "REPLY")
            {
                return RequestToMessageActivity(request);
            }

            return RequestToEventActivity(request);
        }

        public OutgoingMessage ActivityToMessage(Activity activity)
        {
            var messageId = Guid.NewGuid().ToString();

            var message = new OutgoingMessage()
            {
                Name = $"conversations/{activity.Conversation.Id}/messages/{messageId}",
                Fallback = activity.Text,
                MessageId = messageId,
                ContainsRichText = false
            };
            
            var suggestions = ConvertImAndMessageBackSuggestedActionsToSuggestedReplies(activity);
            var additionalSuggestedActions = GetSuggestionsFromActivityAttachments(activity);

            if (additionalSuggestedActions.Any())
            {
                suggestions.AddRange(additionalSuggestedActions);
            }

            if (activity.Attachments?.FirstOrDefault(a => a.ContentType == GoogleAttachmentContentTypes.RichCard) != null)
            {
                // Rich card
                var richCardAttachment = activity.Attachments.First(a => a.ContentType == GoogleAttachmentContentTypes.RichCard);
                message.RichCard = (RichCardContent)richCardAttachment.Content;
                
                if (message.RichCard.StandaloneCard?.CardContent != null)
                {
                    message.RichCard.StandaloneCard.CardContent.Suggestions = suggestions.Take(4).ToList();
                }
            }
            else if (activity.Attachments?.FirstOrDefault(a => a.ContentType == GoogleAttachmentContentTypes.Image) != null)
            {
                // Image
                var attachment = activity.Attachments.First();
                message.Image = new Image() {
                    ContentInfo = new ContentInfo()
                    {
                        AltText = attachment.Name,
                        FileUrl = attachment.ContentUrl
                    }
                };
            }
            else
            {
                // Text
                message.Text = activity.Text;
                message.Suggestions = suggestions;
            }

            return message;
        }

        private Activity RequestToMessageActivity(GoogleBusinessRequest request)
        {
            var activity = Activity.CreateMessageActivity() as Activity;
            activity = SetGeneralActivityProperties(activity, request);
            activity.Text = request.Message?.Text ?? request.SuggestionResponse?.Text;
            return activity;
        }

        private Activity RequestToEventActivity(GoogleBusinessRequest request)
        {
            var activity = Activity.CreateEventActivity() as Activity;
            activity = SetGeneralActivityProperties(activity, request);

            if (request.UserStatus != null)
            {
                if (request.UserStatus.IsTyping)
                {
                    activity.Name = "IsTyping";
                    activity.Value = request;
                }

                if (request.UserStatus.RequestedLiveAgent)
                {
                    activity.Name = "RequestedLiveAgent";
                    activity.Value = request;
                }
            }
            else if (request.Receipts != null)
            {
                activity.Name = request.Receipts.Receipts?.FirstOrDefault()?.ReceiptType;
                activity.Value = request.Receipts.Receipts?.FirstOrDefault()?.Message;
            }
            else if (request.SuggestionResponse != null)
            {
                activity.Name = $"SuggestionResponse";
                activity.Value = request.SuggestionResponse;
            }
            else if (request.SurveyResponse != null)
            {
                activity.Name = $"SurveyResponse";
                activity.Value = request.SurveyResponse;
            }
            else
            {
                activity.Name = "UNKNOWN";
                activity.Value = request;
            }

            return activity;
        }

        private Activity SetGeneralActivityProperties(Activity activity, GoogleBusinessRequest request)
        {
            activity.ChannelId = _options.ChannelId;
            activity.Id = request.RequestId;
            activity.ServiceUrl = _options.ServiceUrl;
            activity.Recipient = new ChannelAccount(request.Agent);
            activity.From = new ChannelAccount(request.ConversationId, request.Context.UserInfo?.DisplayName);
            activity.Conversation = new ConversationAccount(isGroup: false, id: request.ConversationId);
            activity.Timestamp = DateTime.UtcNow;
            activity.ChannelData = request;
            activity.Locale = request.Context.ResolvedLocale;
            return activity;
        }

        private static List<Suggestion> ConvertImAndMessageBackSuggestedActionsToSuggestedReplies(Activity activity)
        {
            var suggestions = new List<Suggestion>();

            if (activity.SuggestedActions != null && activity.SuggestedActions.Actions != null && activity.SuggestedActions.Actions.Any())
            {
                foreach (var suggestion in activity.SuggestedActions.Actions)
                {
                    if (suggestion.Type == ActionTypes.ImBack || suggestion.Type == ActionTypes.MessageBack)
                    {
                        suggestions.Add(new SuggestedReplySuggestion()
                        {
                            Reply = new SuggestedReply()
                            {
                                Text = suggestion.DisplayText ?? suggestion.Text ?? suggestion.Title,
                                PostbackData = suggestion.Text ?? suggestion.Title
                            }
                        });
                    }
                }
            }

            return suggestions;
        }

        private List<Suggestion> GetSuggestionsFromActivityAttachments(Activity activity)
        {
            AttachmentConverter.ConvertAttachments(activity);

            var suggestions = new List<Suggestion>();

            var openUrlAttachments = activity.Attachments?.Where(a => a.ContentType == GoogleAttachmentContentTypes.OpenUrlActionSuggestion).ToList();
            if (openUrlAttachments != null && openUrlAttachments.Any())
            {
                suggestions.AddRange(openUrlAttachments
                    .Select(openUrlAttachment => (OpenUrlActionSuggestion) openUrlAttachment.Content).Cast<Suggestion>()
                    .ToList());
            }

            var dialAttachments = activity.Attachments?.Where(a => a.ContentType == GoogleAttachmentContentTypes.DialActionSuggestion).ToList();
            if (dialAttachments != null && dialAttachments.Any())
            {
                suggestions.AddRange(dialAttachments
                    .Select(dialAttachment => (DialActionSuggestion)dialAttachment.Content).Cast<Suggestion>()
                    .ToList());
            }

            var liveAgentSuggestions = activity.Attachments?.Where(a => a.ContentType == GoogleAttachmentContentTypes.LiveAgentRequestSuggestion).ToList();
            if (liveAgentSuggestions != null && liveAgentSuggestions.Any())
            {
                suggestions.AddRange(liveAgentSuggestions
                    .Select(liveAgentAttachment => (LiveAgentRequestSuggestion)liveAgentAttachment.Content).Cast<Suggestion>()
                    .ToList());
            }

            var authSuggestions = activity.Attachments?.Where(a => a.ContentType == GoogleAttachmentContentTypes.AuthenticationRequestSuggestion).ToList();
            if (authSuggestions != null && authSuggestions.Any())
            {
                suggestions.AddRange(authSuggestions
                    .Select(authAttachment => (LiveAgentRequestSuggestion)authAttachment.Content).Cast<Suggestion>()
                    .ToList());
            }

            return suggestions;
        }
    }
}
