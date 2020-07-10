using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.ActionsSDK.Core;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model;
using Bot.Builder.Community.Adapters.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.ActionsSDK
{
    public class ActionsSdkAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly ILogger _logger;
        private readonly ActionsSdkRequestMapperOptions _actionsSdkRequestMapperOptions;
        private readonly ActionsSdkAdapterOptions _options;

        public ActionsSdkAdapter(ActionsSdkAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new ActionsSdkAdapterOptions();
            _logger = logger ?? NullLogger.Instance;

            _actionsSdkRequestMapperOptions = new ActionsSdkRequestMapperOptions()
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

            string body;
            using (var sr = new StreamReader(httpRequest.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            var actionsSdkRequest = JsonConvert.DeserializeObject<ActionsSdkRequest>(body);
            var actionsSdkRequestMapper = new ActionsSdkRequestMapper(_actionsSdkRequestMapperOptions, _logger);
            var activity = actionsSdkRequestMapper.RequestToActivity(actionsSdkRequest);
            var context = await CreateContextAndRunPipelineAsync(bot, cancellationToken, activity);
            var actionsSdkResponse = actionsSdkRequestMapper.ActivityToResponse(ProcessOutgoingActivities(context.SentActivities), actionsSdkRequest);
            var responseJson = JsonConvert.SerializeObject(actionsSdkResponse, JsonSerializerSettings);
            
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
            return ActivityMappingHelper.MergeActivities(activities);
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ResourceResponse[0]);
        }

        private async Task<TurnContextEx> CreateContextAndRunPipelineAsync(IBot bot, CancellationToken cancellationToken, Activity activity)
        {
            var context = new TurnContextEx(this, activity);
            await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
            return context;
        }
    }
}