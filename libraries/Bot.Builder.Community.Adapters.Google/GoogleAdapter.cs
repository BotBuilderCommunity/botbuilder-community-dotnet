using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google.Core;
using Bot.Builder.Community.Adapters.Google.Core.Helpers;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Bot.Builder.Community.Adapters.Google.Core.Model.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Google
{
    public class GoogleAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly GoogleAdapterOptions _options;
        private readonly ILogger _logger;
        private readonly GoogleRequestMapperOptions _requestMapperOptions;

        public GoogleAdapter(GoogleAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new GoogleAdapterOptions();
            _logger = logger ?? NullLogger.Instance;
            _requestMapperOptions = new GoogleRequestMapperOptions()
            {
                ActionInvocationName = _options.ActionInvocationName,
                ShouldEndSessionByDefault = _options.ShouldEndSessionByDefault
            };
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

            if (_options.ValidateIncomingRequests && !GoogleAuthorizationHandler.ValidateActionProjectId(httpRequest.Headers["Authorization"], _options.ActionProjectId))
            {
                throw new AuthenticationException("Failed to validate incoming request. Project ID in authentication header did not match project ID in AlexaAdapterOptions");
            }

            string body;
            using (var sr = new StreamReader(httpRequest.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            Activity activity;
            string responseJson;

            if (_options.WebhookType == GoogleWebhookType.DialogFlow)
            {
                var dialogFlowRequest = JsonConvert.DeserializeObject<DialogFlowRequest>(body);
                var requestMapper = new DialogFlowRequestMapper(_requestMapperOptions, _logger);
                dialogFlowRequest.OriginalDetectIntentRequest.Payload.EnsureUniqueUserIdInUserStorage();
                activity = requestMapper.RequestToActivity(dialogFlowRequest);
                var context = await CreateContextAndRunPipelineAsync(bot, cancellationToken, activity);
                var response = requestMapper.ActivityToResponse(ProcessOutgoingActivities(context.SentActivities), dialogFlowRequest);
                responseJson = JsonConvert.SerializeObject(response, JsonSerializerSettings);
            }
            else
            {
                var conversationRequest = JsonConvert.DeserializeObject<ConversationRequest>(body);
                var requestMapper = new ConversationRequestMapper(_requestMapperOptions, _logger);
                conversationRequest.EnsureUniqueUserIdInUserStorage();
                activity = requestMapper.RequestToActivity(conversationRequest);
                var context = await CreateContextAndRunPipelineAsync(bot, cancellationToken, activity);
                var response = requestMapper.ActivityToResponse(ProcessOutgoingActivities(context.SentActivities), conversationRequest);
                responseJson = JsonConvert.SerializeObject(response, JsonSerializerSettings);
            }
            
            httpResponse.ContentType = "application/json;charset=utf-8";
            httpResponse.StatusCode = (int)HttpStatusCode.OK;

            var responseData = Encoding.UTF8.GetBytes(responseJson);
            await httpResponse.Body.WriteAsync(responseData, 0, responseData.Length, cancellationToken).ConfigureAwait(false);
        }

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

        public virtual Activity ProcessOutgoingActivities(List<Activity> activities)
        {
            return MappingHelper.MergeActivities(activities);
        }
        
        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ResourceResponse[0]);
        }

        private async Task<TurnContextEx> CreateContextAndRunPipelineAsync(IBot bot, CancellationToken cancellationToken, Activity activity)
        {
            var context = new TurnContextEx(this, activity);
            context.TurnState.Add("GoogleUserId", activity.From.Id);
            await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
            return context;
        }
    }
}