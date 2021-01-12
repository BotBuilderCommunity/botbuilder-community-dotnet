using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Bot.Builder.Community.Adapters.Google.Core.Attachments;
using Bot.Builder.Community.Adapters.Google.Core.Model.Request;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    public class ConversationRequestMapper : GoogleRequestMapperBase
    {
        public ConversationRequestMapper(GoogleRequestMapperOptions options = null, ILogger logger = null) : base(options, logger)
        {
        }

        public Activity RequestToActivity(ConversationRequest request)
        {
            var activity = new Activity();

            var actionIntent =
                request.Inputs.FirstOrDefault(i => i.Intent.ToLowerInvariant().StartsWith("actions.intent"))?.Intent;

            switch (actionIntent?.ToLowerInvariant())
            {
                case "actions.intent.permission":
                case "actions.intent.datetime":
                case "ask_for_sign_in_confirmation":
                case "actions.intent.place":
                case "actions.intent.confirmation":
                    activity.Type = ActivityTypes.Event;
                    activity = SetGeneralActivityProperties(activity, request);
                    activity.Name = actionIntent;
                    activity.Value = request;
                    return activity;
                case "actions.intent.sign_in":
                    activity.Type = ActivityTypes.Event;
                    activity = SetGeneralActivityProperties(activity, request);
                    activity.Name = actionIntent;
                    var signinStatusArgument = request.Inputs.First()?.Arguments?.Where(a => a.Name == "SIGN_IN").FirstOrDefault();
                    var argumentExtension = signinStatusArgument?.Extension;
                    activity.Value = argumentExtension?["status"];
                    return activity;
                case "actions.intent.option":
                    activity.Type = ActivityTypes.Message;
                    activity = SetGeneralActivityProperties(activity, request);
                    activity.Text = StripInvocation(request.Inputs[0]?.RawInputs[0]?.Query,
                        Options.ActionInvocationName);
                    return activity;
            }

            var text = StripInvocation(request.Inputs[0]?.RawInputs[0]?.Query, Options.ActionInvocationName);

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

            _attachmentConverter.ConvertAttachments(activity);

            // Grab any adaptive card attachment to get fallback speak property
            var adaptiveCardAttachment = activity.Attachments.FirstOrDefault(a => a.ContentType == AdaptiveCard.ContentType)?.Content as AdaptiveCard;

            var simpleResponse = new SimpleResponse
            {
                Content = new SimpleResponseContent
                {
                    DisplayText = activity.Text,
                    Ssml = (activity.Speak ?? adaptiveCardAttachment?.Speak),
                    TextToSpeech = activity.Speak == null && adaptiveCardAttachment?.Speak == null ? activity.Text : null
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
                                    Items = new ResponseItem[] { simpleResponse },
                                    Suggestions = ConvertIMAndMessageBackSuggestedActionsToSuggestionChips(activity)?.ToArray()
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
                    Options.ShouldEndSessionByDefault ? InputHints.IgnoringInput : InputHints.ExpectingInput;
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
                    response.ExpectedInputs.First().InputPrompt.RichInitialPrompt.Suggestions = ConvertIMAndMessageBackSuggestedActionsToSuggestionChips(activity)?.ToArray();
                    response.ExpectedInputs.First().InputPrompt.RichInitialPrompt.LinkOutSuggestion = GetLinkOutSuggestionFromActivity(activity);
                    break;
            }

            return response;
        }
    }
}
