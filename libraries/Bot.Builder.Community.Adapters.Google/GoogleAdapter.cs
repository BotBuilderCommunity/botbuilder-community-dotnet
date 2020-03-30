using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Bot.Builder.Community.Adapters.Google.Core;
using Bot.Builder.Community.Adapters.Google.Core.Helpers;
using Bot.Builder.Community.Adapters.Google.Core.Model;
using Bot.Builder.Community.Adapters.Google.Core.Model.Attachments;
using Bot.Builder.Community.Adapters.Google.Model;
using Bot.Builder.Community.Adapters.Google.Model.Attachments;
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
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly GoogleAdapterOptions _options;
        private readonly ILogger _logger;
        private readonly GoogleRequestMapper _requestMapper;

        public GoogleAdapter(GoogleAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new GoogleAdapterOptions();
            _logger = logger ?? NullLogger.Instance;

            _requestMapper = new GoogleRequestMapper(new GoogleRequestMapperOptions()
                { ActionInvocationName = _options.ActionInvocationName, WebhookType = _options.WebhookType });
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
            
            Activity activity = _requestMapper.RequestToActivity(body);

            //if (_options.ValidateIncomingRequests && !GoogleHelper.ValidateRequest(httpRequest, _options.ActionProjectId))
            //{
            //    throw new AuthenticationException("Failed to validate incoming request. Project ID in authentication header did not match project ID in AlexaAdapterOptions");
            //}

            var context = new TurnContextEx(this, activity);

            context.TurnState.Add("GoogleUserId", activity.From.Id);

            await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

            object response = null;

            var outgoingActivity = ProcessOutgoingActivities(context.SentActivities);

            if (_options.WebhookType == GoogleWebhookType.DialogFlow)
            {
                response = _requestMapper.CreateDialogFlowResponseFromActivity(outgoingActivity);
            }
            else
            {
                response = _requestMapper.CreateConversationResponseFromLastActivity(outgoingActivity);
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            httpResponse.ContentType = "application/json;charset=utf-8";
            httpResponse.StatusCode = (int)HttpStatusCode.OK;

            var responseJson = JsonConvert.SerializeObject(response, JsonSerializerSettings);

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
            return _requestMapper.MergeActivities(activities);
        }
        
        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ResourceResponse[0]);
        }
    }
}