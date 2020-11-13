using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Sms.Models;
using Bot.Builder.Community.Adapters.Infobip.Sms.ToActivity;
using Bot.Builder.Community.Adapters.Infobip.Sms.ToInfobip;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
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

namespace Bot.Builder.Community.Adapters.Infobip.Sms
{
    public class InfobipSmsAdapter : InfobipAdapterBase
    {
        private readonly ILogger _logger;
        private readonly ToSmsActivityConverter _toSmsActivityConverter;
        private readonly AuthorizationHelper _authorizationHelper;
        private readonly IInfobipSmsClient _infobipSmsClient;
        private readonly InfobipSmsAdapterOptions _smsAdapterOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipSmsAdapter"/> class using configuration settings.
        /// </summary>
        /// <param name="infobipSmsOptions">Adapter options. Typically created via appsettings loaded into an IConfiguration.</param>
        /// <param name="infobipSmsClient">Client/Proxy used to communicate with Infobip.</param>
        /// <param name="logger">Logger.</param>
        public InfobipSmsAdapter(InfobipSmsAdapterOptions infobipSmsOptions, IInfobipSmsClient infobipSmsClient, ILogger<InfobipSmsAdapter> logger)
        {
            _smsAdapterOptions = infobipSmsOptions ?? throw new ArgumentNullException(nameof(infobipSmsOptions));
            _infobipSmsClient = infobipSmsClient ?? throw new ArgumentNullException(nameof(infobipSmsClient));
            _logger = logger ?? NullLogger<InfobipSmsAdapter>.Instance;

            _toSmsActivityConverter = new ToSmsActivityConverter(_smsAdapterOptions, _logger);
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
                    var messages = ToInfobipSmsConverter.Convert(activity, _smsAdapterOptions);
                    var infobipResponses = new List<InfobipResponseMessage>();

                    foreach (var message in messages)
                    {
                        var currentResponse = await _infobipSmsClient.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);

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

            if (!_authorizationHelper.VerifySignature(httpRequest.Headers[InfobipConstants.HeaderSignatureKey], stringifiedBody, _smsAdapterOptions.InfobipAppSecret))
            {
                if (_smsAdapterOptions.BypassAuthentication)
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

            var incomingMessage = stringifiedBody.FromInfobipIncomingMessageJson<InfobipSmsIncomingResult>();
            var activities = _toSmsActivityConverter.Convert(incomingMessage);

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
