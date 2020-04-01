using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Google.Core.Helpers;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Bot.Builder.Community.Adapters.Google.Model;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
                ChannelId = _options.ChannelId,
                ServiceUrl = _options.ServiceUrl,
                Recipient = new ChannelAccount("", "action"),
                Conversation = new ConversationAccount(false, "conversation", $"{payload.Conversation.ConversationId}"),
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Locale = payload.User.Locale,
                Value = payload.Inputs[0]?.Intent,
                ChannelData = payload,
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
                activity.Text = MappingHelper.StripInvocation(payload.Inputs[0]?.RawInputs[0]?.Query, _options.ActionInvocationName);
            }

            if (payload.Inputs.FirstOrDefault()?.Arguments?.FirstOrDefault()?.Name == "OPTION")
            {
                activity.Value = payload.Inputs.First().Arguments.First().TextValue;
            }

            activity.ChannelData = payload;

            return activity;
        }

        public ConversationWebhookResponse ActivityToResponse(Activity activity, ConversationRequest request)
        {
            var response = new ConversationWebhookResponse { UserStorage = request.User.UserStorage };

            if (activity == null || activity.Type != ActivityTypes.Message)
            {
                response.ExpectUserResponse = false;
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
                                    PossibleIntents = new ISystemIntent[]
                                    {
                                        new TextIntent(),
                                    },
                                    InputPrompt = new InputPrompt()
                                    {
                                        RichInitialPrompt = new RichResponse() {Items = responseItems.ToArray()}
                                    }
                                }
                        };

                    var suggestionChips = MappingHelper.ConvertSuggestedActivitiesToSuggestionChips(activity.SuggestedActions);
                    if (suggestionChips.Any())
                    {
                        response.ExpectedInputs.First().InputPrompt.RichInitialPrompt.Suggestions =
                            suggestionChips.ToArray();
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
