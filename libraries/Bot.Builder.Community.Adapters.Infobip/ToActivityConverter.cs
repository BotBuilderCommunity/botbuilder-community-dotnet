using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Infobip
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
                                            $"DoneAt={response.DoneAt}, SentAt={response.SentAt}");

                var activity = new Activity
                {
                    Type = ActivityTypes.Event,
                    Id = response.MessageId,
                    Name = InfobipReportTypes.DELIVERY,
                    Timestamp = response.DoneAt,
                    ChannelId = InfobipConstants.ChannelName,
                    Conversation = new ConversationAccount { Id = response.To },
                    From = new ChannelAccount { Id = _adapterOptions.InfobipWhatsAppNumber },
                    Recipient = new ChannelAccount { Id = response.To },
                    Text = null,
                    ChannelData = response,
                    Entities = new List<Entity>()
                };

                if (!string.IsNullOrWhiteSpace(response.CallbackData))
                {
                    var serialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.CallbackData);
                    activity.Entities.Add(new InfobipCallbackData(serialized));
                }

                return activity;
            }

            if (response.IsSeenReport())
            {
                _logger.Log(LogLevel.Debug, $"Received SEEN notification: MessageId={response.MessageId}, " +
                                            $"SeenAt={response.SeenAt}, SentAt={response.SentAt}");

                return new Activity
                {
                    Type = ActivityTypes.Event,
                    Id = response.MessageId,
                    Name = InfobipReportTypes.SEEN,
                    Timestamp = response.SeenAt,
                    ChannelId = InfobipConstants.ChannelName,
                    Conversation = new ConversationAccount { Id = response.To },
                    From = new ChannelAccount { Id = response.From },
                    Recipient = new ChannelAccount { Id = response.To },
                    Text = null,
                    ChannelData = response
                };
            }

            if (response.IsMessage())
            {
                _logger.Log(LogLevel.Debug, $"MO message received: MessageId={response.MessageId}, " +
                                            $"IntegrationType={response.IntegrationType}, " +
                                            $"receivedAt={response.ReceivedAt}");

                // handle any type of WA message sent by subscriber, can be: TEXT, IMAGE, DOCUMENT, LOCATION, CONTACT, VIDEO
                // - https://dev-old.infobip.com/whatsapp-business-messaging/incoming-whatsapp-messages
                return (Activity) await ConvertMessageToMessageActivity(response);
            }

            throw new Exception("Unsupported message received - not DLR, SEEN or MO message: \n" +
                                JsonConvert.SerializeObject(response, Formatting.Indented));
        }

        private async Task<IActivity> ConvertMessageToMessageActivity(InfobipIncomingResult response)
        {
            var activity = Activity.CreateMessageActivity();

            activity.Id = response.MessageId;
            activity.ChannelId = InfobipConstants.ChannelName;
            activity.ChannelData = response;
            activity.Recipient = new ChannelAccount { Id = response.To };
            activity.From = new ChannelAccount { Id = response.From };
            activity.Conversation = new ConversationAccount { IsGroup = false, Id = response.From };
            activity.Timestamp = response.ReceivedAt;

            if (response.Message.Type == InfobipIncomingMessageTypes.Text)
            {
                activity.Text = response.Message.Text;
                activity.TextFormat = TextFormatTypes.Plain;
            }
            else if (response.Message.Type == InfobipIncomingMessageTypes.Location)
            {
                activity.Entities.Add(new GeoCoordinates
                {
                    Latitude = response.Message.Latitude,
                    Longitude = response.Message.Longitude
                });
            }
            else if (response.Message.IsMedia())
            {
                var contentType = await _infobipClient.GetContentTypeAsync(response.Message.Url.AbsoluteUri).ConfigureAwait(false);
                activity.Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = contentType,
                        ContentUrl = response.Message.Url.AbsoluteUri,
                        Name = response.Message.Caption
                    }
                };
            }
            else
            {
                _logger.Log(LogLevel.Information, $"Received MO message: {response.MessageId} has unsupported message type");
                return null;
            }

            return activity;
        }
    }
}
