using Bot.Builder.Community.Adapters.MessageBird.Models;
using Bot.Builder.Community.Adapters.Shared;
using MessageBird;
using MessageBird.Objects.Conversations;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.MessageBird
{
    public class MessageBirdAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly ILogger _logger;
        private readonly MessageBirdAdapterOptions _options;
        private readonly MessageBirdRequestAuthorization _requestAuthorization;


        public MessageBirdAdapter(MessageBirdAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? NullLogger.Instance;

            if (_options.UseWhatsAppSandbox)
            {
                _messageBirdClient = Client.CreateDefault(_options.AccessKey, features: new Client.Features[] { Client.Features.EnableWhatsAppSandboxConversations });
            }
            else
            {
                _messageBirdClient = Client.CreateDefault(_options.AccessKey);
            }
            _requestAuthorization = new MessageBirdRequestAuthorization();
        }
        Client _messageBirdClient;
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

            if (!_requestAuthorization.Verify(httpRequest.Headers["Messagebird-Signature"], _options.SigningKey, httpRequest.Headers["Messagebird-Request-Timestamp"], body))
            {
                httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            ToActivityConverter _ac = new ToActivityConverter(_options, _logger);
            var messageBirdRequest = JsonConvert.DeserializeObject<MessageBirdWebhookPayload>(body);
            var activities = _ac.Convert(messageBirdRequest); 
            foreach (var activity in activities)
            {
                using (var context = new TurnContext(this, activity))
                {
                    await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                }
            }
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

        public virtual Task<Activity> ProcessOutgoingActivitiesAsync(List<Activity> activities, ITurnContext turnContext)
        {
            return Task.FromResult(ActivityMappingHelper.MergeActivities(activities));
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var activity in activities.Where(a => a.Type == ActivityTypes.Message))
                {
                    var messages = ToMessageBirdConverter.Convert(activity);
                    foreach (var message in messages)
                    {
                        ConversationMessage _response = _messageBirdClient.SendConversationMessage(message.conversationId, message.conversationMessageRequest);
                    }
                }
                return Task.FromResult(new ResourceResponse[0]);


            }
            catch (Exception)
            {
                return Task.FromResult(new ResourceResponse[0]);

            }
        }
    }
}
