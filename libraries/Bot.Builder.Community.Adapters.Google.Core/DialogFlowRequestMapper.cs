using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Google.Core.Helpers;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    public class DialogFlowRequestMapper
    {
        private readonly GoogleRequestMapperOptions _options;
        private ILogger _logger;

        public DialogFlowRequestMapper(GoogleRequestMapperOptions options = null, ILogger logger = null)
        {
            _options = options ?? new GoogleRequestMapperOptions();
            _logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(DialogFlowRequest request)
        {
            var payload = request.OriginalDetectIntentRequest.Payload;

            var activity = new Activity
            {
                DeliveryMode = DeliveryModes.ExpectReplies,
                ChannelId = _options.ChannelId,
                ServiceUrl = _options.ServiceUrl,
                Recipient = new ChannelAccount("", "action"),
                Conversation = new ConversationAccount(false, "conversation", $"{payload.Conversation.ConversationId}"),
                From = new ChannelAccount(payload.GetUserIdFromUserStorage()),
                Id = request.ResponseId,
                Timestamp = DateTime.UtcNow,
                Locale = payload.User.Locale,
                ChannelData = request,
                Text = MappingHelper.StripInvocation(payload.Inputs[0]?.RawInputs[0]?.Query,
                    _options.ActionInvocationName)
            };


            if (string.IsNullOrEmpty(activity.Text))
            {
                activity.Type = ActivityTypes.ConversationUpdate;
                activity.MembersAdded = new List<ChannelAccount>() { new ChannelAccount() { Id = activity.From.Id } };
            }
            else
            {
                activity.Type = ActivityTypes.Message;
            }

            if (payload.Inputs.FirstOrDefault()?.Arguments?.FirstOrDefault()?.Name == "OPTION")
            {
                activity.Value = payload.Inputs.First().Arguments.First().TextValue;
            }

            activity.ChannelData = payload;

            return activity;
        }

        public DialogFlowResponse ActivityToResponse(Activity activity, DialogFlowRequest dialogFlowRequest)
        {
            var response = new DialogFlowResponse()
            {
                Payload = new ResponsePayload()
                {
                    Google = new PayloadContent()
                    {
                        RichResponse = new RichResponse(),
                        ExpectUserResponse = !_options.ShouldEndSessionByDefault,
                        UserStorage = dialogFlowRequest.OriginalDetectIntentRequest.Payload.User.UserStorage
                    }
                }
            };

            if (activity == null || activity.Type != ActivityTypes.Message)
            {
                response.Payload.Google.ExpectUserResponse = false;
                return response;
            }
            
            var simpleResponse = new SimpleResponse
            {
                Content = new SimpleResponseContent
                {
                    DisplayText = activity.Text,
                    Ssml = activity.Speak,
                    TextToSpeech = activity.Text
                }
            };

            var responseItems = new List<Item> { simpleResponse };

            response.Payload.Google.RichResponse.Items = responseItems.ToArray();

            // If suggested actions have been added to outgoing activity
            // add these to the response as Google Suggestion Chips

            if (activity.InputHint == null || activity.InputHint == InputHints.AcceptingInput)
            {
                activity.InputHint =
                    _options.ShouldEndSessionByDefault ? InputHints.IgnoringInput : InputHints.ExpectingInput;
            }

            // check if we should be listening for more input from the user
            switch (activity.InputHint)
            {
                case InputHints.IgnoringInput:
                    response.Payload.Google.ExpectUserResponse = false;
                    break;
                case InputHints.ExpectingInput:
                    response.Payload.Google.ExpectUserResponse = true;

                    var suggestionChips = MappingHelper.ConvertSuggestedActivitiesToSuggestionChips(activity.SuggestedActions);
                    if (suggestionChips.Any())
                    {
                        response.Payload.Google.RichResponse.Suggestions = suggestionChips.ToArray();
                    }
                    break;
            }

            return response;
        }

        public static Activity MergeActivities(IList<Activity> activities)
        {
            return MappingHelper.MergeActivities(activities);
        }
    }
}
