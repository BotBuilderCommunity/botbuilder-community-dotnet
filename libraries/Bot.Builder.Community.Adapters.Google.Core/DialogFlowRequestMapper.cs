using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Google.Core.Attachments;
using Bot.Builder.Community.Adapters.Google.Core.Model.Request;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    public class DialogFlowRequestMapper : GoogleRequestMapperBase
    {
        public DialogFlowRequestMapper(GoogleRequestMapperOptions options = null, ILogger logger = null) : base(options, logger)
        {
            Options = options ?? new GoogleRequestMapperOptions();
            Logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(DialogFlowRequest request)
        {
            var payload = request.OriginalDetectIntentRequest.Payload;

            var activity = new Activity();
            activity = SetGeneralActivityProperties(activity, payload);
            var actionIntent = payload.Inputs.FirstOrDefault(i => i.Intent.ToLowerInvariant().StartsWith("actions.intent"))?.Intent;
            var queryText = StripInvocation(payload.Inputs[0]?.RawInputs[0]?.Query, Options.ActionInvocationName);

            if (request.QueryResult.Intent.IsFallback)
            {
                if (string.IsNullOrEmpty(queryText))
                {
                    activity.Type = ActivityTypes.ConversationUpdate;
                    activity.MembersAdded = new List<ChannelAccount>() { new ChannelAccount() { Id = activity.From.Id } };
                    return activity;
                }
                activity.Type = ActivityTypes.Message;
                activity.Text = queryText;
                return activity;
            }

            switch (actionIntent?.ToLowerInvariant())
            {
                case "actions.intent.sign_in":
                    activity.Type = ActivityTypes.Event;
                    activity.Name = request.QueryResult.Intent.DisplayName;
                    var signinStatusArgument = request.OriginalDetectIntentRequest.Payload.Inputs.First()?.Arguments?.Where(a => a.Name == "SIGN_IN").FirstOrDefault();
                    var argumentExtension = signinStatusArgument?.Extension;
                    activity.Value = argumentExtension?["status"];
                    return activity;
                case "actions.intent.option":
                case "actions.intent.text":
                    activity.Type = ActivityTypes.Message;
                    activity.Text = queryText;
                    return activity;
                case "actions.intent.permission":
                case "actions.intent.datetime":
                case "ask_for_sign_in_confirmation":
                case "actions.intent.place":
                case "actions.intent.confirmation":
                default:
                    activity.Type = ActivityTypes.Event;
                    activity.Name = request.QueryResult.Intent.DisplayName;
                    activity.Value = request;
                    return activity;
            }
        }

        public DialogFlowResponse ActivityToResponse(Activity activity, DialogFlowRequest dialogFlowRequest)
        {
            var response = new DialogFlowResponse()
            {
                Payload = new ResponsePayload()
                {
                    Google = new PayloadContent()
                    {
                        ExpectUserResponse = !Options.ShouldEndSessionByDefault,
                        UserStorage = dialogFlowRequest.OriginalDetectIntentRequest.Payload.User.UserStorage
                    }
                }
            };

            // Send default empty response if no activity or invalid activity type sent
            if (activity == null || activity.Type != ActivityTypes.Message)
            {
                response.Payload.Google.ExpectUserResponse = false;
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
                response.Payload.Google.ExpectUserResponse = true;
                response.Payload.Google.SystemIntent = GetDialogFlowSystemIntentFromSystemIntent(processedIntentStatus);

                if (processedIntentStatus.AllowAdditionalInputPrompt)
                {
                    response.Payload.Google.RichResponse = new RichResponse()
                    {
                        Items = new ResponseItem[] {simpleResponse}
                    };
                }

                return response;
            }
            
            var responseItems = new List<ResponseItem> { simpleResponse };
            responseItems.AddRange(GetResponseItemsFromActivityAttachments(activity));

            response.Payload.Google.RichResponse = new RichResponse()
            {
                Items = responseItems.ToArray()
            };

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
                    response.Payload.Google.ExpectUserResponse = false;
                    break;
                case InputHints.ExpectingInput:
                    response.Payload.Google.ExpectUserResponse = true;

                    var suggestionChips = ConvertSuggestedActionsToSuggestionChips(activity);
                    if (suggestionChips.Any())
                    {
                        response.Payload.Google.RichResponse.Suggestions = suggestionChips.ToArray();
                    }
                    break;
            }

            return response;
        }

        private DialogFlowSystemIntent GetDialogFlowSystemIntentFromSystemIntent(
            ProcessHelperIntentAttachmentsResult processedIntentStatus)
        {
            var dialogFlowSystemIntent = new DialogFlowSystemIntent()
            {
                Intent = processedIntentStatus.Intent.Intent
            };

            switch (processedIntentStatus.Intent)
            {
                case SigninIntent signinIntent:
                    dialogFlowSystemIntent.Data = signinIntent.InputValueData;
                    break;
                case ListIntent listIntent:
                    dialogFlowSystemIntent.Data = listIntent.InputValueData;
                    break;
                case CarouselIntent carouselIntent:
                    dialogFlowSystemIntent.Data = carouselIntent.InputValueData;
                    break;
                case PermissionsIntent permissionsIntent:
                    dialogFlowSystemIntent.Data = permissionsIntent.InputValueData;
                    break;
                case DateTimeIntent dateTimeIntent:
                    dialogFlowSystemIntent.Data = dateTimeIntent.InputValueData;
                    break;
                case PlaceLocationIntent placeLocationIntent:
                    dialogFlowSystemIntent.Data = placeLocationIntent.InputValueData;
                    break;
                case ConfirmationIntent confirmationIntent:
                    dialogFlowSystemIntent.Data = confirmationIntent.InputValueData;
                    break;
            }

            return dialogFlowSystemIntent;
        }
    }
}
