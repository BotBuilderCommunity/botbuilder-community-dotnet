using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.ToActivity
{
    public class ToMessagesActivityConverter
    {
        private readonly ILogger _logger;
        private readonly InfobipMessagesAdapterOptions _messagesAdapterOptions;
        private readonly IInfobipMessagesClient _infobipMessagesClient;

        public ToMessagesActivityConverter(InfobipMessagesAdapterOptions messagesAdapterOptions, IInfobipMessagesClient infobipMessagesClient, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messagesAdapterOptions = messagesAdapterOptions ?? throw new ArgumentNullException(nameof(messagesAdapterOptions));
            _infobipMessagesClient = infobipMessagesClient ?? throw new ArgumentNullException(nameof(infobipMessagesClient));
        }

        /// <summary>
        /// Converts a single Infobip message to a Bot Framework activity.
        /// </summary>
        /// <param name="infobipIncomingMessage">The message to be processed.</param>
        /// <returns>An Activity with the result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="infobipIncomingMessage"/> is null.</exception>
        /// <remarks>A webhook call may deliver more than one message at a time.</remarks>
        public async Task<IEnumerable<Activity>> Convert(InfobipIncomingMessage<InfobipMessagesIncomingResult> infobipIncomingMessage)
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

        private async Task<Activity> ConvertToActivity(InfobipMessagesIncomingResult response)
        {
            if (response.Error != null)
            {
                if (response.Error.Id > 0)
                    throw new Exception($"{response.Error.Name} {response.Error.Description}");
            }

            if (response.IsDeliveryReport())
            {
                _logger.Log(LogLevel.Debug, $"Received DLR notification: MessageId={response.MessageId}, " +
                                            $"DoneAt={response.DoneAt}, SentAt={response.SentAt}, Channel={response.Channel}");

                var activity = InfobipMessagesDeliveryReportToActivity.Convert(response);
                HandleCallbackData(response, activity);

                return activity;
            }

            if (response.IsSeenReport())
            {
                _logger.Log(LogLevel.Debug, $"Received SEEN notification: MessageId={response.MessageId}, " +
                                            $"SeenAt={response.SeenAt}, SentAt={response.SentAt}");

                return InfobipMessagesSeenReportToActivity.Convert(response);
            }

            if (response.IsMessage())
            {
                _logger.Log(LogLevel.Debug, $"MO message received: MessageId={response.MessageId}, " +
                                            $"IntegrationType={response.IntegrationType}, " +
                                            $"receivedAt={response.ReceivedAt}");

                var activity = await InfobipMessagesToActivity.Convert(response, _infobipMessagesClient);
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

        private static void HandleCallbackData(InfobipMessagesIncomingResult response, Activity activity)
        {
            if (string.IsNullOrWhiteSpace(response.CallbackData)) return;
            try
            {
                var serialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.CallbackData);
                activity.AddInfobipCallbackData(serialized);
            }
            catch (JsonException)
            {
                // If callback data is not valid JSON, treat it as a string
                activity.AddInfobipCallbackData(new Dictionary<string, string> { { "data", response.CallbackData } });
            }
        }
    }
}