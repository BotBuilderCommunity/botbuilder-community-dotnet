using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Communication.Sms;
using Bot.Builder.Community.Adapters.ACS.SMS.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.ACS.SMS
{
    /// <summary>
    /// A <see cref="BotAdapter"/> that can connect to Azure Communication Services via SMS.
    /// </summary>
    public class AcsSmsAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private readonly AcsSmsAdapterOptions _options;
        private readonly ILogger _logger;
        private readonly AcsSmsRequestMapper _requestMapper;

        public AcsSmsAdapter(AcsSmsAdapterOptions options = null, ILogger logger = null)
        {
            _options = options ?? new AcsSmsAdapterOptions();
            _logger = logger ?? NullLogger.Instance;
            _requestMapper = new AcsSmsRequestMapper(new AcsSmsRequestMapperOptions()
            {
                AcsPhoneNumber = options.AcsPhoneNumber,
                EnableDeliveryReports = options.EnableDeliveryReports
            });
        }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot,
            CancellationToken cancellationToken = default)
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

            var eventGridSubscriber = new EventGridSubscriber();
            var eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(body);
            
            foreach (var eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.Data is SubscriptionValidationEventData eventData)
                {
                    // Do any additional validation (as required) and then return back the below response
                    var responseData = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = eventData.ValidationCode
                    };
                    httpResponse.StatusCode = (int)HttpStatusCode.OK;
                    var responseJson = JsonConvert.SerializeObject(responseData);
                    var responseDataBytes = Encoding.UTF8.GetBytes(responseJson);
                    await httpResponse.Body.WriteAsync(responseDataBytes, 0, responseDataBytes.Length).ConfigureAwait(false);
                }
                else
                {
                    var activity = RequestToActivity(eventGridEvent);

                    using (var context = new TurnContext(this, activity))
                    {
                        context.TurnState.Add("httpStatus", HttpStatusCode.OK.ToString("D"));
                        await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                        httpResponse.StatusCode = (int)HttpStatusCode.OK;
                    }
                }
            }
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
        public async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic,
            CancellationToken cancellationToken)
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

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity,
            CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(
                new NotImplementedException("ACS SMS adapter does not support updateActivity."));
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference,
            CancellationToken cancellationToken)
        {
            return Task.FromException(new NotImplementedException("ACS SMS adapter does not support deleteActivity."));
        }

        public virtual Activity RequestToActivity(EventGridEvent request)
        {
            return _requestMapper.RequestToActivity(request);
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext,
            Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();
            foreach (var activity in activities)
            {
                if (activity.Type != ActivityTypes.Message)
                {
                    _logger.LogTrace(
                        $"Unsupported Activity Type: '{activity.Type}'. Only Activities of type 'Message' are supported.");
                }
                else
                {
                    var sendSmsRequest = _requestMapper.ActivityToResponse(activity);
                    var smsClient = new SmsClient(_options.AcsConnectionString,
                        new SmsClientOptions(SmsClientOptions.ServiceVersion.V1, _options.RetryOptions));

                    var sendSmsResponse = smsClient.Send(
                        sendSmsRequest.From,
                        sendSmsRequest.To,
                        sendSmsRequest.Message,
                        new SendSmsOptions()
                        {
                            EnableDeliveryReport = sendSmsRequest.EnableDeliveryReport
                        });

                    var response = new ResourceResponse()
                    {
                        Id = sendSmsResponse.Value.MessageId,
                    };

                    responses.Add(response);
                }
            }

            return responses.ToArray();
        }

        /// <summary>
        /// Sends a proactive message from the bot to a conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initialize a conversation with a bot
        /// before the bot can send activities to the user.
        /// <para>This method registers the following services for the turn.<list type="bullet">
        /// <item><description><see cref="IIdentity"/> (key = "BotIdentity"), a claims claimsIdentity for the bot.
        /// </description></item>
        /// </list></para>
        /// </remarks>
        /// <seealso cref="BotAdapter.RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public override async Task ContinueConversationAsync(ClaimsIdentity claimsIdentity,
            ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            using (var context = new TurnContext(this, reference.GetContinuationActivity()))
            {
                context.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);
                context.TurnState.Add<BotCallbackHandler>(callback);
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
