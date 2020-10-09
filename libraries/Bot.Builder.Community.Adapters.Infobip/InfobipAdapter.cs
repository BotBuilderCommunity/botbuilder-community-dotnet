using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Bot.Builder.Community.Adapters.Infobip.ToActivity;
using Bot.Builder.Community.Adapters.Infobip.ToInfobip;

namespace Bot.Builder.Community.Adapters.Infobip
{
    public class InfobipAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private readonly ILogger _logger;
        private readonly ToActivityConverter _toActivityConverter;
        private readonly AuthorizationHelper _authorizationHelper;
        private readonly IInfobipClient _infobipClient;
        private readonly InfobipAdapterOptions _infobipOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipAdapter"/> class using configuration settings.
        /// </summary>
        /// <param name="infobipOptions">Adapter options. Typically created via appsettings loaded into an IConfiguration.</param>
        /// <param name="infobipClient">Client/Proxy used to communicate with Infobip.</param>
        /// <param name="logger">Logger.</param>
        public InfobipAdapter(InfobipAdapterOptions infobipOptions, IInfobipClient infobipClient, ILogger<InfobipAdapter> logger)
        {
            _infobipOptions = infobipOptions ?? throw new ArgumentNullException(nameof(infobipOptions));
            _infobipClient = infobipClient ?? throw new ArgumentNullException(nameof(infobipClient));
            _logger = logger ?? NullLogger<InfobipAdapter>.Instance;

            _toActivityConverter = new ToActivityConverter(_infobipOptions, _infobipClient, _logger);
            _authorizationHelper = new AuthorizationHelper();
        }

        /// <summary>
        /// Standard BotBuilder adapter method to send a message from the bot to the messaging API.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activities">An array of outgoing activities to be sent back to the messaging API.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>An array of <see cref="ResourceResponse"/> objects containing the IDs that Infobip assigned to the sent messages.</returns>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext,
            Activity[] activities,
            CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    var messages = ToInfobipConverter.Convert(activity, _infobipOptions);
                    var infobipResponses = new List<InfobipResponseMessage>();

                    foreach (var message in messages)
                    {
                        var currentResponse = await _infobipClient.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);

                        if (currentResponse == null) continue;

                        _logger.Log(LogLevel.Debug,
                            $"Received MT submit response: MessageId={currentResponse.MessageId}, " +
                            $"Status={JsonConvert.SerializeObject(currentResponse.Status)}");

                        infobipResponses.Add(currentResponse);
                    }

                    responses.Add(new InfobipResourceResponse
                    {
                        Id = string.Join("|", infobipResponses.Select(x => x.MessageId.ToString())),
                        ActivityId = activity.Id,
                        ResponseMessages = infobipResponses
                    });
                }
            }

            return responses.ToArray();
        }

        /// <summary>
        /// Accepts an incoming webhook request, creates a turn context,
        /// and runs the middleware pipeline for an incoming TRUSTED activity.
        /// </summary>
        /// <param name="httpRequest">Represents the incoming side of an HTTP request.</param>
        /// <param name="httpResponse">Represents the outgoing side of an HTTP request.</param>
        /// <param name="bot">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot,
            CancellationToken cancellationToken = new CancellationToken())
        {
            string stringifiedBody;

            using (var sr = new StreamReader(httpRequest.Body))
            {
                stringifiedBody = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            if (!_authorizationHelper.VerifySignature(httpRequest.Headers[InfobipConstants.HeaderSignatureKey], stringifiedBody, _infobipOptions.InfobipAppSecret))
            {
                if (_infobipOptions.BypassAuthentication)
                {
                    _logger.LogWarning("WARNING: Bypassing authentication. Do not run this in production.");
                }
                else
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    _logger.LogWarning("WARNING: Webhook received message with invalid signature. Request stopped and response will have unauthorized status code!");
                    return;
                }
            }

            var incomingMessage = stringifiedBody.FromInfobipIncomingMessageJson();
            var activities = await _toActivityConverter.Convert(incomingMessage).ConfigureAwait(false);

            foreach (var activity in activities)
            {
                using (var context = new TurnContext(this, activity))
                {
                    await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Throws a <see cref="NotImplementedException"/> exception in all cases.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity,
            CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(
                new NotImplementedException("Infobip adapter does not support updateActivity."));
        }

        /// <summary>
        /// Throws a <see cref="NotImplementedException"/> exception in all cases.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="reference">Conversation reference for the activity to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference,
            CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(
                new NotImplementedException("Infobip adapter does not support updateActivity."));
        }
    }
}
