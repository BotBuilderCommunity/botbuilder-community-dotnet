using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Alexa.Attachments;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Alexa
{
    public class AlexaAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        internal const string BotIdentityKey = "BotIdentity";

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly AlexaAdapterOptions _options;
        private readonly ILogger _logger;

        public AlexaAdapter(AlexaAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new AlexaAdapterOptions();
            _logger = logger ?? NullLogger.Instance;
        }

        private Dictionary<string, List<Activity>> Responses { get; set; }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
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

            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(body, JsonSerializerSettings);

            if (skillRequest.Version != "1.0")
            {
                throw new Exception($"Unexpected request version of '{skillRequest.Version}' received.");
            }

            if (_options.ValidateIncomingAlexaRequests
                && !await AlexaHelper.ValidateRequest(httpRequest, skillRequest, body, _logger))
            {
                throw new AuthenticationException("Failed to validate incoming request.");
            }

            var alexaResponse = await ProcessActivity(skillRequest, bot.OnTurnAsync);

            if (alexaResponse == null)
            {
                throw new ArgumentNullException(nameof(alexaResponse));
            }

            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode = (int)HttpStatusCode.OK;

            var responseJson = JsonConvert.SerializeObject(alexaResponse, JsonSerializerSettings);

            var responseData = Encoding.UTF8.GetBytes(responseJson);
            await httpResponse.Body.WriteAsync(responseData, 0, responseData.Length, cancellationToken).ConfigureAwait(false);
        }

        public async Task<SkillResponse> ProcessActivity(SkillRequest alexaRequest, BotCallbackHandler callback)
        {
            var activity = RequestToActivity(alexaRequest);
            var context = new TurnContext(this, activity);

            Responses = new Dictionary<string, List<Activity>>();

            await RunPipelineAsync(context, callback, default).ConfigureAwait(false);

            if (context.GetAlexaRequestBody().Request.Type == "SessionEndedRequest")
            {
                return ResponseBuilder.Tell(string.Empty);
            }

            var key = $"{activity.Conversation.Id}:{activity.Id}";

            try
            {
                var activities = Responses.ContainsKey(key) ? Responses[key] : new List<Activity>();
                var response = CreateResponseFromActivity(processMultipleActivities(activities), context);
                return response;
            }
            finally
            {
                if (Responses.ContainsKey(key))
                {
                    Responses.Remove(key);
                }
            }
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var resourceResponses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                        var conversation = activity.Conversation ?? new ConversationAccount();
                        var key = $"{conversation.Id}:{activity.ReplyToId}";

                        if (Responses.ContainsKey(key))
                        {
                            Responses[key].Add(activity);
                        }
                        else
                        {
                            Responses[key] = new List<Activity> { activity };
                        }

                        break;
                    default:
                        _logger.LogTrace($"Unsupported Activity Type: '{activity.Type}'. Only Activities of type 'Message' or 'Event' are supported.");
                        break;
                }

                resourceResponses.Add(new ResourceResponse(activity.Id));
            }

            return Task.FromResult(resourceResponses.ToArray());
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(new NotImplementedException("Alexa adapter does not support updateActivity."));
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            return Task.FromException(new NotImplementedException("Alexa adapter does not support deleteActivity."));
        }

        private static Activity RequestToActivity(SkillRequest skillRequest)
        {
            var system = skillRequest.Context.System;

            var activity = new Activity
            {
                ChannelId = "alexa",
                ServiceUrl = $"{system.ApiEndpoint}?token ={system.ApiAccessToken}",
                Recipient = new ChannelAccount(system.Application.ApplicationId, "skill"),
                From = new ChannelAccount(system.User.UserId, "user"),
                Conversation = new ConversationAccount(false, "conversation", skillRequest.Session.SessionId),
                Type = skillRequest.Request.Type,
                Id = skillRequest.Request.RequestId,
                Timestamp = skillRequest.Request.Timestamp,
                Locale = skillRequest.Request.Locale,
                ChannelData = skillRequest
            };

            return activity;
        }

        private static void ProcessActivityAttachments(Activity activity, SkillResponse response)
        {
            var card = activity.Attachments?.FirstOrDefault(a => a.GetType() == typeof(CardAttachment)) as CardAttachment;
            if (card != null)
            {
                response.Response.Card = card.Card;
            }

            var directiveAttachments = activity.Attachments?.Where(a => a.GetType() == typeof(DirectiveAttachment))
                .Select(d => d as DirectiveAttachment);
            var directives = directiveAttachments?.Select(d => d.Directive).ToList();
            if (directives != null && directives.Any())
            {
                response.Response.Directives = directives;
            }
        }

        private SkillResponse CreateResponseFromActivity(Activity activity, ITurnContext context)
        {
            var response = new SkillResponse()
            {
                Version = "1.0",
                Response = new ResponseBody(),
                SessionAttributes = context.AlexaSessionAttributes()
            };

            if (!SecurityElement.IsValidText(activity.Text))
            {
                activity.Text = SecurityElement.Escape(activity.Text);
            }

            if (!string.IsNullOrEmpty(activity.Speak))
            {
                activity.Speak = processSuggestedActions(activity, activity.Speak);

                response.Response.OutputSpeech =
                    new SsmlOutputSpeech(
                        activity.Speak.Contains("<speak>")
                            ? activity.Speak
                            : $"<speak>{activity.Speak}</speak>");
            }
            else if (!string.IsNullOrEmpty(activity.Text))
            {
                activity.Text = processSuggestedActions(activity, activity.Text);

                response.Response.OutputSpeech = new SsmlOutputSpeech(
                    "<speak>" + activity.Text + "</speak>");
            }

            ProcessActivityAttachments(activity, response);

            if (AlexaHelper.ShouldSetEndSession(response))
            {
                switch (activity.InputHint)
                {
                    case InputHints.IgnoringInput:
                        response.Response.ShouldEndSession = true;
                        break;
                    case InputHints.ExpectingInput:
                        response.Response.ShouldEndSession = false;
                        response.Response.Reprompt = new Reprompt(activity.Text);
                        break;
                    default:
                        response.Response.ShouldEndSession = _options.ShouldEndSessionByDefault;
                        break;
                }
            }

            return response;
        }

        private Activity processMultipleActivities(List<Activity> activities)
        {
            Activity resultActivity = activities.Last();
            if (_options.TryConcatMultipleTextActivties && activities.Count() > 1)
            {   
                for (int i = activities.Count - 2; i >= 0; i--)
                {
                    if (!string.IsNullOrEmpty(activities[i].Speak))
                    {
                        activities[i].Speak = activities[i].Speak.Trim(new char[] { ' ', '.' });
                        resultActivity.Text = string.Format("{0}. {1}", activities[i].Speak, resultActivity.Text);

                    }
                    else if (!string.IsNullOrEmpty(activities[i].Text)) {
                        activities[i].Text = activities[i].Text.Trim(new char[] { ' ', '.'});
                        resultActivity.Text = string.Format("{0}. {1}", activities[i].Text, resultActivity.Text);
                    }
                }
            }
            return resultActivity;
        }

        private string processSuggestedActions(Activity activity, string text)
        {
            if (_options.TryConvertSuggestedActionsToText && activity.SuggestedActions?.Actions != null && activity.SuggestedActions.Actions.Count() > 0)
            {
                for (int i = 0; i < activity.SuggestedActions.Actions.Count(); i++)
                {
                    text = string.Format("{0} ({1}) {2}", text, i+1, activity.SuggestedActions.Actions[i].Value);
                }
            }
            return text;
        }
    }
}
