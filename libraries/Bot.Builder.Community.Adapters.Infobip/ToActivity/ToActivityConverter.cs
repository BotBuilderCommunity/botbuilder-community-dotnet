using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.ToActivity
{
    public class ToActivityConverter
    {
        private readonly ILogger _logger;
        private readonly InfobipAdapterOptions _adapterOptions;
        private readonly IInfobipClient _infobipClient;

        public ToActivityConverter(InfobipAdapterOptions adapterOptions, IInfobipClient infobipClient, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _adapterOptions = adapterOptions ?? throw new ArgumentNullException(nameof(adapterOptions));
            _infobipClient = infobipClient ?? throw new ArgumentNullException(nameof(infobipClient));
        }

        /// <summary>
        /// Converts a single Infobip message to a Bot Framework activity.
        /// </summary>
        /// <param name="infobipIncomingMessage">The message to be processed.</param>
        /// <returns>An Activity with the result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="infobipIncomingMessage"/> is null.</exception>
        /// <remarks>A webhook call may deliver more than one message at a time.</remarks>
        public async Task<IEnumerable<Activity>> Convert(InfobipIncomingMessage infobipIncomingMessage)
        {
            if (infobipIncomingMessage == null) throw new ArgumentNullException(nameof(infobipIncomingMessage));

            if (infobipIncomingMessage.Results == null || !infobipIncomingMessage.Results.Any())
            {
                _logger.LogError("WebHookResponse has no results");
                throw new ArgumentOutOfRangeException("No data from webhook",
                    new Exception("No data received from webhook at " + DateTime.UtcNow));
            }

            var result = new List<Activity>();

            foreach (var message in infobipIncomingMessage.Results)
            {
                try
                {
                    var activity = await ConvertToActivity(message);
                    if (activity != null)
                        result.Add(activity);
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, "Error handling message response: " + e.Message, e);
                }
            }

            return result;
        }

        private async Task<Activity> ConvertToActivity(InfobipIncomingResult response)
        {
            if (response.Error != null)
            {
                //error codes - https://dev-old.infobip.com/getting-started/response-status-and-error-codes
                if (response.Error.Id > 0)
                    throw new Exception($"{response.Error.Name} {response.Error.Description}");
            }

            if (response.IsDeliveryReport())
            {
                _logger.Log(LogLevel.Debug, $"Received DLR notification: MessageId={response.MessageId}, " +
                                            $"DoneAt={response.DoneAt}, SentAt={response.SentAt}, Channel={response.Channel}");

                var activity = InfobipDeliveryReportToActivity.Convert(response, _adapterOptions);
                if (string.IsNullOrEmpty(activity.ChannelId))
                    _logger.Log(LogLevel.Error, $"{response.Channel} is not supported channel");
                HandleCallbackData(response, activity);

                return activity;
            }

            if (response.IsSeenReport())
            {
                _logger.Log(LogLevel.Debug, $"Received SEEN notification: MessageId={response.MessageId}, " +
                                            $"SeenAt={response.SeenAt}, SentAt={response.SentAt}");

                return InfobipSeenReportToActivity.Convert(response);
            }

            if (response.IsSmsMessage())
            {
                _logger.Log(LogLevel.Debug, $"MO message received: MessageId={response.MessageId}, " +
                                            $"IntegrationType=SMS, " +
                                            $"receivedAt={response.ReceivedAt}");

                var activity = InfobipSmsToActivity.Convert(response);
                HandleCallbackData(response, activity);

                return activity;
            }

            if (response.IsWhatsAppMessage())
            {
                _logger.Log(LogLevel.Debug, $"MO message received: MessageId={response.MessageId}, " +
                                            $"IntegrationType={response.IntegrationType}, " +
                                            $"receivedAt={response.ReceivedAt}");

                var activity = await InfobipWhatsAppToActivity.Convert(response, _infobipClient);
                if (activity == null)
                {
                    _logger.Log(LogLevel.Information, $"Received MO message: {response.MessageId} has unsupported message type");
                    return null;
                }

                HandleCallbackData(response, activity);

                return activity;
            }

            throw new Exception("Unsupported message received - not DLR, SEEN or MO message: \n" +
                                JsonConvert.SerializeObject(response, Formatting.Indented));
        }

        private static void HandleCallbackData(InfobipIncomingResult response, Activity activity)
        {
            if (string.IsNullOrWhiteSpace(response.CallbackData)) return;
            var serialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.CallbackData);
            activity.AddInfobipCallbackData(serialized);
        }
    }
}
