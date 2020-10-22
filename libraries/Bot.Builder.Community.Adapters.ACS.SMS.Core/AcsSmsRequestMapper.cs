using Microsoft.Extensions.Logging;
using System;
using Azure.Communication;
using Bot.Builder.Community.Adapters.ACS.SMS.Core.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Bot.Schema;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.ACS.SMS.Core
{
    public class AcsSmsRequestMapper
    {
        private readonly AcsSmsRequestMapperOptions _options;
        private readonly ILogger _logger;

        public AcsSmsRequestMapper(AcsSmsRequestMapperOptions options = null, ILogger logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.AcsPhoneNumber))
            {
                throw new NullReferenceException($"No ACS phone number was provided on {nameof(options)}");
            }

            _logger = logger ?? NullLogger.Instance;
        }

        public Activity RequestToActivity(EventGridEvent eventGridEvent)
        {
            Activity activity;

            switch (eventGridEvent.EventType)
            {
                case "Microsoft.Communication.SMSReceived":
                    var smsReceivedRequestData = JsonConvert.DeserializeObject<SmsReceivedRequestData>(eventGridEvent.Data.ToString());
                    activity = Activity.CreateMessageActivity() as Activity;
                    SetGeneralActivityProperties(activity, eventGridEvent);
                    activity.Conversation = new ConversationAccount(false, id: $"{smsReceivedRequestData.From}");
                    activity.From = new ChannelAccount($"{smsReceivedRequestData.From}");
                    activity.Text = smsReceivedRequestData.Message;
                    activity.Recipient = new ChannelAccount(smsReceivedRequestData.To, "bot");
                    break;
                case "Microsoft.Communication.SMSDeliveryReportReceived":
                    var smsDeliveryReportReceivedRequestData = JsonConvert.DeserializeObject<SmsDeliveryReportReceivedRequestData>(eventGridEvent.Data.ToString());
                    activity = Activity.CreateEventActivity() as Activity;
                    SetGeneralActivityProperties(activity, eventGridEvent);
                    activity.Name = eventGridEvent.EventType.ToString();
                    activity.Value = eventGridEvent;
                    activity.Conversation = new ConversationAccount(false, id: $"{smsDeliveryReportReceivedRequestData.From}");
                    activity.From = new ChannelAccount($"{smsDeliveryReportReceivedRequestData.From}");
                    activity.Recipient = new ChannelAccount(smsDeliveryReportReceivedRequestData.To, "bot");
                    break;
                default:
                    throw new NotSupportedException($"Events of type {eventGridEvent.EventType} are not currently supported.");
            }

            return activity;
        }

        public Activity SetGeneralActivityProperties(Activity activity, EventGridEvent request)
        {
            activity.ChannelId = _options.ChannelId;
            activity.ServiceUrl = _options.ServiceUrl;
            activity.Id = request.Id.ToString();
            activity.Timestamp = DateTime.UtcNow;
            activity.ChannelData = request;
            return activity;
        }

        public AcsSendSmsRequest ActivityToResponse(Activity activity)
        {
            var sendSmsRequest = new AcsSendSmsRequest()
            {
                From = new PhoneNumber(_options.AcsPhoneNumber),
                To = new PhoneNumber(activity.Recipient.Id.StartsWith("+") ? activity.Recipient.Id : $"+{activity.Recipient.Id}"),
                Message = activity.Text,
                EnableDeliveryReport = _options.EnableDeliveryReports
            };

            return sendSmsRequest;
        }
    }
}
