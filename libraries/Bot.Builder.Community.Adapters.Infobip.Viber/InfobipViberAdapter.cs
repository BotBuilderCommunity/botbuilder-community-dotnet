using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.ToActivity;
using Bot.Builder.Community.Adapters.Infobip.Viber.ToInfobip;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Viber
{
    public class InfobipViberAdapter: InfobipAdapterBase
    {
        private readonly ILogger _logger;
        private readonly ToViberActivityConverter _toViberActivityConverter;
        private readonly AuthorizationHelper _authorizationHelper;
        private readonly IInfobipViberClient _infobipViberClient;
        private readonly InfobipViberAdapterOptions _infobipViberOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipViberAdapter"/> class using configuration settings.
        /// </summary>
        /// <param name="infobipViberOptions">Adapter options. Typically created via appsettings loaded into an IConfiguration.</param>
        /// <param name="viberClient">Client/Proxy used to communicate with Infobip.</param>
        /// <param name="logger">Logger.</param>
        public InfobipViberAdapter(InfobipViberAdapterOptions infobipViberOptions, IInfobipViberClient viberClient, ILogger<InfobipViberAdapter> logger)
        {
            _infobipViberOptions = infobipViberOptions ?? throw new ArgumentNullException(nameof(infobipViberOptions));
            _infobipViberClient = viberClient ?? throw new ArgumentNullException(nameof(viberClient));
            _logger = logger ?? NullLogger<InfobipViberAdapter>.Instance;

            _toViberActivityConverter = new ToViberActivityConverter(infobipViberOptions, logger);
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
                    var messages = ToViberInfobipConverter.Convert(activity, _infobipViberOptions.InfobipViberScenarioKey);
                    var infobipResponses = new List<InfobipResponseMessage>();

                    foreach (var message in messages)
                    {
                        var currentResponse = await _infobipViberClient.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);

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
        public override async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot,
            CancellationToken cancellationToken = new CancellationToken())
        {
            string stringifiedBody;

            using (var sr = new StreamReader(httpRequest.Body))
            {
                stringifiedBody = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            if (!_authorizationHelper.VerifySignature(httpRequest.Headers[InfobipConstants.HeaderSignatureKey], stringifiedBody, _infobipViberOptions.InfobipAppSecret))
            {
                if (_infobipViberOptions.BypassAuthentication)
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

            var incomingMessage = stringifiedBody.FromInfobipIncomingMessageJson<InfobipViberIncomingResult>();
            var activities = _toViberActivityConverter.Convert(incomingMessage);

            foreach (var activity in activities)
            {
                using (var context = new TurnContext(this, activity))
                {
                    await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
