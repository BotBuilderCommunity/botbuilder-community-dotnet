using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google.Model;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Google
{
    public class GoogleAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        internal const string BotIdentityKey = "BotIdentity";

        private static readonly JsonParser _dialogFlowJsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly GoogleAdapterOptions _options;
        private readonly ILogger _logger;
        private Dictionary<string, List<Activity>> _responses;

        public GoogleAdapter(GoogleAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new GoogleAdapterOptions();
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            string body;
            using (var sr = new StreamReader(httpRequest.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            if (_options.WebhookType == GoogleWebhookType.DialogFlow)
            {
                var dialogFlowRequest = _dialogFlowJsonParser.Parse<WebhookRequest>(body);
            }

            Activity activity = null;

            if (_options.WebhookType == GoogleWebhookType.DialogFlow)
            {
                var dialogFlowRequest = JsonConvert.DeserializeObject<DialogFlowRequest>(body);
                activity = DialogFlowRequestToActivity(dialogFlowRequest);
            }
            else
            {
                if (_options.ValidateIncomingRequests && !GoogleHelper.ValidateRequest(httpRequest, _options.ActionProjectId))
                {
                    throw new AuthenticationException("Failed to validate incoming request. Project ID in authentication header did not match project ID in AlexaAdapterOptions");
                }

                var actionPayload = JsonConvert.DeserializeObject<ActionsPayload>(body);
                activity = PayloadToActivity(actionPayload);
            }

            var googleResponse = await ProcessActivityAsync(activity, bot.OnTurnAsync);

            if (googleResponse == null)
            {
                throw new ArgumentNullException(nameof(googleResponse));
            }

            if (googleResponse == null)
            {
                throw new ArgumentNullException(nameof(googleResponse));
            }

            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode = (int)HttpStatusCode.OK;

            var responseJson = JsonConvert.SerializeObject(googleResponse, JsonSerializerSettings);

            var responseData = Encoding.UTF8.GetBytes(responseJson);
            await httpResponse.Body.WriteAsync(responseData, 0, responseData.Length, cancellationToken).ConfigureAwait(false);
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken CancellationToken)
        {
            var resourceResponses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                    case ActivityTypes.EndOfConversation:
                        var conversation = activity.Conversation ?? new ConversationAccount();
                        var key = $"{conversation.Id}:{activity.ReplyToId}";

                        if (_responses.ContainsKey(key))
                        {
                            _responses[key].Add(activity);
                        }
                        else
                        {
                            _responses[key] = new List<Activity> { activity };
                        }

                        break;
                    default:
                        Trace.WriteLine(
                            $"GoogleAdapter.SendActivities(): Activities of type '{activity.Type}' aren't supported.");
                        break;
                }

                resourceResponses.Add(new ResourceResponse(activity.Id));
            }

            return Task.FromResult(resourceResponses.ToArray());
        }

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="logic">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most channels require a user to initiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="BotAdapter.RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        /// <exception cref="ArgumentNullException"><paramref name="reference"/> or
        /// <paramref name="logic"/> is <c>null</c>.</exception>
        public async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic, CancellationToken cancellationToken)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (logic == null)
            {
                throw new ArgumentNullException(nameof(logic));
            }

            var request = reference.GetContinuationActivity().ApplyConversationReference(reference, true);

            using (var context = new TurnContext(this, request))
            {
                await RunPipelineAsync(context, logic, cancellationToken).ConfigureAwait(false);
            }
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task<object> ProcessActivityAsync(Activity activity, BotCallbackHandler logic, string uniqueRequestId = null)
        {
            var context = new TurnContext(this, activity);

            context.TurnState.Add("GoogleUserId", activity.From.Id);

            _responses = new Dictionary<string, List<Activity>>();

            await RunPipelineAsync(context, logic, default).ConfigureAwait(false);

            var key = $"{activity.Conversation.Id}:{activity.Id}";

            try
            {
                object response = null;
                var activities = _responses.ContainsKey(key) ? _responses[key] : new List<Activity>();

                if (_options.WebhookType == GoogleWebhookType.DialogFlow)
                {
                    response = CreateDialogFlowResponseFromLastActivity(activities, context);
                }
                else
                {
                    response = CreateConversationResponseFromLastActivity(activities, context);
                }

                return response;
            }
            finally
            {
                if (_responses.ContainsKey(key))
                {
                    _responses.Remove(key);
                }
            }
        }

        private Activity DialogFlowRequestToActivity(DialogFlowRequest request)
        {
            var activity = PayloadToActivity(request.OriginalDetectIntentRequest.Payload);
            activity.ChannelData = request;
            return activity;
        }

        private Activity PayloadToActivity(ActionsPayload payload)
        {
            var activity = new Activity
            {
                ChannelId = "google",
                ServiceUrl = $"",
                Recipient = new ChannelAccount("", "action"),
                Conversation = new ConversationAccount(false, "conversation",
                    $"{payload.Conversation.ConversationId}"),
                Type = ActivityTypes.Message,
                Text = GoogleHelper.StripInvocation(payload.Inputs[0]?.RawInputs[0]?.Query, _options.ActionInvocationName),
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Locale = payload.User.Locale,
                Value = payload.Inputs[0]?.Intent,
                ChannelData = payload
            };

            if (!string.IsNullOrEmpty(payload.User.UserId))
            {
                activity.From = new ChannelAccount(payload.User.UserId, "user");
            }
            else
            {
                if (!string.IsNullOrEmpty(payload.User.UserStorage))
                {
                    var values = JObject.Parse(payload.User.UserStorage);
                    if (values.ContainsKey("UserId"))
                    {
                        activity.From = new ChannelAccount(values["UserId"].ToString(), "user");
                    }
                }
                else
                {
                    activity.From = new ChannelAccount(Guid.NewGuid().ToString(), "user");
                }
            }

            if (payload.Inputs.FirstOrDefault()?.Arguments?.FirstOrDefault()?.Name == "OPTION")
            {
                activity.Text = payload.Inputs.First().Arguments.First().TextValue;
            }

            activity.ChannelData = payload;

            return activity;
        }

        private ConversationWebhookResponse CreateConversationResponseFromLastActivity(List<Activity> activities, ITurnContext context)
        {
            var activity = ProcessOutgoingActivities(activities);

            if (!SecurityElement.IsValidText(activity.Text))
            {
                activity.Text = SecurityElement.Escape(activity.Text);
            }

            var response = new ConversationWebhookResponse();

            var userStorage = new JObject
            {
                { "UserId", context.TurnState["GoogleUserId"].ToString() }
            };
            response.UserStorage = userStorage.ToString();

            if (activity?.Attachments != null
                && activity.Attachments.FirstOrDefault(a => a.ContentType == SigninCard.ContentType) != null)
            {
                response.ExpectUserResponse = true;
                response.ResetUserStorage = null;

                response.ExpectedInputs = new ExpectedInput[]
                {
                    new ExpectedInput()
                    {
                        PossibleIntents = new PossibleIntent[]
                        {
                            new PossibleIntent()
                            {
                                Intent = "actions.intent.SIGN_IN",
                                InputValueData = new InputValueData()
                                {
                                    type = "type.googleapis.com/google.actions.v2.SignInValueSpec"
                                }
                            },
                        },
                        InputPrompt = new InputPrompt()
                        {
                            RichInitialPrompt = new RichResponse()
                            {
                                Items = new Item[]
                                {
                                    new SimpleResponse()
                                    {
                                        Content = new SimpleResponseContent()
                                        {
                                            TextToSpeech = "PLACEHOLDER"
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                return response;
            }

            if (!string.IsNullOrEmpty(activity?.Text))
            {
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

                // Add Google card to response if set
                AddCardToResponse(context, ref responseItems, activity);

                // Add Media response to response if set
                AddMediaResponseToResponse(context, ref responseItems, activity);

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
                                    PossibleIntents = new PossibleIntent[]
                                    {
                                        new PossibleIntent() {Intent = "actions.intent.TEXT"},
                                    },
                                    InputPrompt = new InputPrompt()
                                    {
                                        RichInitialPrompt = new RichResponse() {Items = responseItems.ToArray()}
                                    }
                                }
                            };

                        var suggestionChips = AddSuggestionChipsToResponse(context);
                        if (suggestionChips.Any())
                        {
                            response.ExpectedInputs.First().InputPrompt.RichInitialPrompt.Suggestions =
                                suggestionChips.ToArray();
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                response.ExpectUserResponse = false;
            }

            return response;
        }

        private DialogFlowResponse CreateDialogFlowResponseFromLastActivity(List<Activity> activities, ITurnContext context)
        {
            var activity = ProcessOutgoingActivities(activities);

            var response = new DialogFlowResponse()
            {
                Payload = new ResponsePayload()
                {
                    Google = new PayloadContent()
                    {
                        RichResponse = new RichResponse(),
                        ExpectUserResponse = !_options.ShouldEndSessionByDefault
                    }
                }
            };

            if (!string.IsNullOrEmpty(activity?.Text))
            {
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

                // Add Google card to response if set
                AddCardToResponse(context, ref responseItems, activity);

                // Add Media response to response if set
                AddMediaResponseToResponse(context, ref responseItems, activity);

                response.Payload.Google.RichResponse.Items = responseItems.ToArray();

                // If suggested actions have been added to outgoing activity
                // add these to the response as Google Suggestion Chips
                var suggestionChips = AddSuggestionChipsToResponse(context);
                if (suggestionChips.Any())
                {
                    response.Payload.Google.RichResponse.Suggestions = suggestionChips.ToArray();
                }

                if (context.TurnState.ContainsKey("systemIntent"))
                {
                    var optionSystemIntent = context.TurnState.Get<ISystemIntent>("systemIntent");
                    response.Payload.Google.SystemIntent = optionSystemIntent;
                }

                // check if we should be listening for more input from the user
                switch (activity.InputHint)
                {
                    case InputHints.IgnoringInput:
                        response.Payload.Google.ExpectUserResponse = false;
                        break;
                    case InputHints.ExpectingInput:
                        response.Payload.Google.ExpectUserResponse = true;
                        break;
                    case InputHints.AcceptingInput:
                    default:
                        break;
                }
            }
            else
            {
                response.Payload.Google.ExpectUserResponse = false;
            }

            return response;
        }

        private void AddMediaResponseToResponse(ITurnContext context, ref List<Item> responseItems, Activity activity)
        {
            if (context.TurnState.ContainsKey("GoogleMediaResponse") && context.TurnState["GoogleMediaResponse"] is MediaResponse)
            {
                responseItems.Add(context.TurnState.Get<MediaResponse>("GoogleMediaResponse"));
            }
        }

        private static List<Suggestion> AddSuggestionChipsToResponse(ITurnContext context)
        {
            var suggestionChips = new List<Suggestion>();

            if (context.TurnState.ContainsKey("GoogleSuggestionChips") && context.TurnState["GoogleSuggestionChips"] is List<Suggestion>)
            {
                suggestionChips.AddRange(context.TurnState.Get<List<Suggestion>>("GoogleSuggestionChips"));
            }

            if (context.Activity.SuggestedActions != null && context.Activity.SuggestedActions.Actions.Any())
            {
                foreach (var suggestion in context.Activity.SuggestedActions.Actions)
                {
                    suggestionChips.Add(new Suggestion { Title = suggestion.Title });
                }
            }

            return suggestionChips;
        }

        private void AddCardToResponse(ITurnContext context, ref List<Item> responseItems, Activity activity)
        {
            if (context.TurnState.ContainsKey("GoogleCard") && context.TurnState["GoogleCard"] is GoogleBasicCard)
            {
                responseItems.Add(context.TurnState.Get<GoogleBasicCard>("GoogleCard"));
            }
            else if (_options.TryConvertFirstActivityAttachmentToGoogleCard)
            {
                //TODO: Implement automatic conversion from hero card to Google basic card
                //CreateAlexaCardFromAttachment(activity, response);
            }
        }

        private Activity ProcessOutgoingActivities(List<Activity> activities)
        {
            if (activities.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(activities));
            }

            if (activities.Count() > 1)
            {
                switch (_options.MultipleOutgoingActivitiesPolicy)
                {
                    case MultipleOutgoingActivitiesPolicies.TakeFirstActivity:
                        return activities.First();
                    case MultipleOutgoingActivitiesPolicies.TakeLastActivity:
                        return activities.Last();
                    case MultipleOutgoingActivitiesPolicies.ConcatenateTextSpeakPropertiesFromAllActivities:
                        var resultActivity = activities.Last();

                        for (int i = activities.Count - 2; i >= 0; i--)
                        {
                            if (!string.IsNullOrEmpty(activities[i].Speak))
                            {
                                activities[i].Speak = activities[i].Speak.Trim(new char[] { ' ', '.' });
                                resultActivity.Text = string.Format("{0}. {1}", activities[i].Speak, resultActivity.Text);

                            }
                            else if (!string.IsNullOrEmpty(activities[i].Text))
                            {
                                activities[i].Text = activities[i].Text.Trim(new char[] { ' ', '.' });
                                resultActivity.Text = string.Format("{0}. {1}", activities[i].Text, resultActivity.Text);
                            }
                        }

                        return resultActivity;
                }
            }

            return activities.Last();
        }
    }
}