using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.ToActivity;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.ToInfobip;
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

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp
{
    public class InfobipWhatsAppAdapter : InfobipAdapterBase
    {
        private readonly ILogger _logger;
        private readonly ToWhatsAppActivityConverter _toWhatsAppActivityConverter;
        private readonly AuthorizationHelper _authorizationHelper;
        private readonly IInfobipWhatsAppClient _infobipWhatsAppClient;
        private readonly InfobipWhatsAppAdapterOptions _whatsAppAdapterOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfobipWhatsAppAdapter"/> class using configuration settings.
        /// </summary>
        /// <param name="infobipWhatsAppOptions">Adapter options. Typically created via appsettings loaded into an IConfiguration.</param>
        /// <param name="infobipWhatsAppClient">Client/Proxy used to communicate with Infobip.</param>
        /// <param name="logger">Logger.</param>
        public InfobipWhatsAppAdapter(InfobipWhatsAppAdapterOptions infobipWhatsAppOptions, IInfobipWhatsAppClient infobipWhatsAppClient, ILogger<InfobipWhatsAppAdapter> logger)
        {
            _whatsAppAdapterOptions = infobipWhatsAppOptions ?? throw new ArgumentNullException(nameof(infobipWhatsAppOptions));
            _infobipWhatsAppClient = infobipWhatsAppClient ?? throw new ArgumentNullException(nameof(infobipWhatsAppClient));
            _logger = logger ?? NullLogger<InfobipWhatsAppAdapter>.Instance;

            _toWhatsAppActivityConverter = new ToWhatsAppActivityConverter(_whatsAppAdapterOptions, _infobipWhatsAppClient, _logger);
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
                    var messages = ToWhatsAppInfobipConverter.Convert(activity, _whatsAppAdapterOptions);
                    var infobipResponses = new List<InfobipResponseMessage>();

                    foreach (var message in messages)
                    {
                        var currentResponse = await _infobipWhatsAppClient.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);

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

            if (!_authorizationHelper.VerifySignature(httpRequest.Headers[InfobipConstants.HeaderSignatureKey], stringifiedBody, _whatsAppAdapterOptions.InfobipAppSecret))
            {
                if (_whatsAppAdapterOptions.BypassAuthentication)
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

            var incomingMessage = stringifiedBody.FromInfobipIncomingMessageJson<InfobipWhatsAppIncomingResult>();
            var activities = await _toWhatsAppActivityConverter.Convert(incomingMessage).ConfigureAwait(false);

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
