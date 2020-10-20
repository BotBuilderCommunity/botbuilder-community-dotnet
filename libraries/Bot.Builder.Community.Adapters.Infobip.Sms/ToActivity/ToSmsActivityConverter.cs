using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Sms.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Adapters.Infobip.Sms.ToActivity
{
    public class ToSmsActivityConverter
    {
        private readonly ILogger _logger;
        private readonly InfobipSmsAdapterOptions _smsAdapterOptions;

        public ToSmsActivityConverter(InfobipSmsAdapterOptions smsAdapterOptions, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _smsAdapterOptions = smsAdapterOptions ?? throw new ArgumentNullException(nameof(smsAdapterOptions));
        }

        /// <summary>
        /// Converts a single Infobip message to a Bot Framework activity.
        /// </summary>
        /// <param name="infobipIncomingMessage">The message to be processed.</param>
        /// <returns>An Activity with the result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="infobipIncomingMessage"/> is null.</exception>
        /// <remarks>A webhook call may deliver more than one message at a time.</remarks>
        public IEnumerable<Activity> Convert(InfobipIncomingMessage<InfobipSmsIncomingResult> infobipIncomingMessage)
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
                    var activity = ConvertToActivity(message);
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

        private Activity ConvertToActivity(InfobipSmsIncomingResult response)
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

                var activity = InfobipSmsDeliveryReportToActivity.Convert(response, _smsAdapterOptions.InfobipSmsNumber);
                if (string.IsNullOrEmpty(activity.ChannelId))
                    _logger.Log(LogLevel.Error, $"{response.Channel} is not supported channel");
                HandleCallbackData(response, activity);

                return activity;
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

            throw new Exception("Unsupported message received - not DLR, SEEN or MO message: \n" +
                                JsonConvert.SerializeObject(response, Formatting.Indented));
        }

        private static void HandleCallbackData(InfobipSmsIncomingResult response, Activity activity)
        {
            if (string.IsNullOrWhiteSpace(response.CallbackData)) return;
            var serialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.CallbackData);
            activity.AddInfobipCallbackData(serialized);
        }
    }
}
