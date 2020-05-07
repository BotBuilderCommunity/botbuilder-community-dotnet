using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ConversationRequestMapper
    {
        private readonly GoogleRequestMapperOptions _options;
        private ILogger _logger;

        public ConversationRequestMapper(GoogleRequestMapperOptions options = null, ILogger logger = null)
        {
            _options = options ?? new GoogleRequestMapperOptions();
            _logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(ConversationRequest payload)
        {
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                DeliveryMode = DeliveryModes.ExpectReplies,
                ChannelId = _options.ChannelId,
                ServiceUrl = _options.ServiceUrl,
                Recipient = new ChannelAccount("", "action"),
                Conversation = new ConversationAccount(false, id: $"{payload.Conversation.ConversationId}"),
                From = new ChannelAccount(payload.GetUserIdFromUserStorage()),
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Locale = payload.User.Locale,
                ChannelData = payload,
                Text = MappingHelper.StripInvocation(payload.Inputs[0]?.RawInputs[0]?.Query,
                    _options.ActionInvocationName)
            };

            if (string.IsNullOrEmpty(activity.Text))
            {
                activity.Type = ActivityTypes.ConversationUpdate;
                activity.MembersAdded = new List<ChannelAccount>() { new ChannelAccount() { Id = activity.From.Id } };
            }

            return activity;
        }

        public ConversationWebhookResponse ActivityToResponse(Activity activity, ConversationRequest request)
        {
            var response = new ConversationWebhookResponse
            {
                UserStorage = request.User.UserStorage
            };

            // Send default empty response if no activity or invalid activity type sent
            if (activity == null || activity.Type != ActivityTypes.Message)
            {
                response.ExpectUserResponse = false;
                return response;
            }

            activity.ConvertAttachmentContent();

            var simpleResponse = new SimpleResponse
            {
                Content = new SimpleResponseContent
                {
                    DisplayText = activity.Text,
                    Ssml = activity.Speak,
                    TextToSpeech = activity.Text
                }
            };

            var processedIntentStatus = ProcessHelperIntentAttachments(activity);

            // If we have a system intent to send - send it - with or without additional simple prompt
            if (processedIntentStatus.Intent != null)
            {
                response.ExpectUserResponse = true;
                response.ExpectedInputs = new ExpectedInput[]
                {
                    new ExpectedInput()
                    {
                        PossibleIntents = new SystemIntent[]
                        {
                            processedIntentStatus.Intent
                        },
                        InputPrompt = processedIntentStatus.AllowAdditionalInputPrompt
                            ? new InputPrompt()
                                {
                                    RichInitialPrompt = new RichResponse()
                                    {
                                        Items = new ResponseItem[] { simpleResponse }
                                    }
                                }
                            : null
                    }
                };

                return response;
            };

            // We haven't sent a response using a SystemIntent, so send simple response
            // plus any card activities
            var responseItems = new List<ResponseItem> { simpleResponse };
            responseItems.AddRange(GetResponseItemsFromActivityAttachments(activity));

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
                    response.ExpectUserResponse = false;
                    response.FinalResponse = new FinalResponse()
                    {
                        RichResponse = new RichResponse() { Items = responseItems.ToArray() }
                    };
                    break;
                case InputHints.ExpectingInput:
                    response.ExpectUserResponse = true;
                    response.ExpectedInputs = new ExpectedInput[]
                        {
                                new ExpectedInput()
                                {
                                    PossibleIntents = new SystemIntent[]
                                    {
                                        new TextIntent(),
                                    },
                                    InputPrompt = new InputPrompt()
                                    {
                                        RichInitialPrompt = new RichResponse() {Items = responseItems.ToArray()}
                                    }
                                }
                        };
                    ProcessSuggestedActions(activity, response);
                    break;
            }

            return response;
        }

        public static Activity MergeActivities(IList<Activity> activities)
        {
            return MappingHelper.MergeActivities(activities);
        }

        private void ProcessSuggestedActions(Activity activity, ConversationWebhookResponse response)
        {
            // Process SuggestedActions
            var suggestionChips = MappingHelper.ConvertSuggestedActivitiesToSuggestionChips(activity.SuggestedActions);
            if (suggestionChips.Any())
            {
                response.ExpectedInputs.First().InputPrompt.RichInitialPrompt.Suggestions =
                    suggestionChips.ToArray();
            }
        }

        private ProcessHelperIntentAttachmentsResult ProcessHelperIntentAttachments(Activity activity)
        {
            var result = new ProcessHelperIntentAttachmentsResult();

            if (activity.Attachments.FirstOrDefault(a =>
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

            if (activity.Attachments.FirstOrDefault(a =>
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

            if (activity.Attachments.FirstOrDefault(a =>
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

            if (activity.Attachments.FirstOrDefault(a =>
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

            if (activity.Attachments.FirstOrDefault(a =>
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

            if (activity.Attachments.FirstOrDefault(a =>
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

            if (activity.Attachments.FirstOrDefault(a =>
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

        private List<ResponseItem> GetResponseItemsFromActivityAttachments(Activity activity)
        {
            var responseItems = new List<ResponseItem>();

            activity.ConvertAttachmentContent();

            responseItems.Add(ProcessResponseItemAttachment<BasicCard>(GoogleAttachmentContentTypes.BasicCard, activity));
            responseItems.Add(ProcessResponseItemAttachment<TableCard>(GoogleAttachmentContentTypes.TableCard, activity));
            responseItems.Add(ProcessResponseItemAttachment<MediaResponse>(GoogleAttachmentContentTypes.MediaResponse, activity));

            return responseItems;
        }

        private static ResponseItem ProcessResponseItemAttachment<T>(string contentType, Activity activity) where T : ResponseItem
        {
            return activity.Attachments?
                .Where(a => a.ContentType == contentType)
                .Select(a => (T)a.Content).FirstOrDefault();
        }

        private static SystemIntent ProcessSystemIntentAttachment<T>(string contentType, Activity activity) where T : SystemIntent
        {
            return activity.Attachments?
                .Where(a => a.ContentType == contentType)
                .Select(a => (T)a.Content).FirstOrDefault();
        }
    }

    internal class ProcessHelperIntentAttachmentsResult
    {
        public SystemIntent Intent { get; set; }
        public bool AllowAdditionalInputPrompt { get; set; }
    }
}
