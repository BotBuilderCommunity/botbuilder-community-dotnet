using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Alexa.Core;
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
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly AlexaAdapterOptions _options;
        private readonly ILogger _logger;
        private readonly AlexaRequestMapper _requestMapper;

        public AlexaAdapter(AlexaAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new AlexaAdapterOptions();
            _logger = logger ?? NullLogger.Instance;

            _requestMapper = new AlexaRequestMapper(new AlexaRequestMapperOptions
            {
                ShouldEndSessionByDefault = _options.ShouldEndSessionByDefault
            });
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

            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(body, JsonSerializerSettings);

            if (skillRequest.Version != "1.0")
            {
                throw new Exception($"Unexpected request version of '{skillRequest.Version}' received.");
            }

            if (_options.ValidateIncomingAlexaRequests
                && !await ValidationHelper.ValidateRequest(httpRequest, skillRequest, body, _options.AlexaSkillId, _logger))
            {
                throw new AuthenticationException("Failed to validate incoming request.");
            }

            var alexaResponse = await ProcessAlexaRequestAsync(skillRequest, bot.OnTurnAsync);

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
            return Task.FromException<ResourceResponse>(new NotImplementedException("Alexa adapter does not support updateActivity."));
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            return Task.FromException(new NotImplementedException("Alexa adapter does not support deleteActivity."));
        }

        private async Task<SkillResponse> ProcessAlexaRequestAsync(SkillRequest alexaRequest, BotCallbackHandler logic)
        {
            var activity = RequestToActivity(alexaRequest);
            var context = new TurnContextEx(this, activity);

            await RunPipelineAsync(context, logic, default).ConfigureAwait(false);

            var activities = context.SentActivities;

            var outgoingActivity = ProcessOutgoingActivities(activities);

            var response = _requestMapper.ActivityToResponse(outgoingActivity, alexaRequest);

            return response;
        }

        public virtual Activity ProcessOutgoingActivities(List<Activity> activities)
        {
            return _requestMapper.MergeActivities(activities);
        }

        public virtual Activity RequestToActivity(SkillRequest request)
        {
            return _requestMapper.RequestToActivity(request);
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ResourceResponse[0]);
        }
    }
}
