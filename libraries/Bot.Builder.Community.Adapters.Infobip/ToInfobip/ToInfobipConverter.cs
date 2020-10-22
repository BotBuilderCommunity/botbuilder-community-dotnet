using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;
using Microsoft.Rest;

namespace Bot.Builder.Community.Adapters.Infobip.ToInfobip
{
    public class ToInfobipConverter
    {
        /// <summary>
        /// Converts a Bot Framework activity to a Infobip OMNI failover message ready for the OMNI API.
        /// </summary>
        /// <param name="activity">The activity to be converted to Infobip OMNI failover message.</param>
        /// <param name="options">The Infobip options object which contains scenario keys.</param>
        /// <returns>The resulting messages.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is null.</exception>
        /// <exception cref="ValidationException"><paramref name="activity"/> is null.</exception>
        public static List<InfobipOmniFailoverMessage> Convert(Activity activity, InfobipAdapterOptions options)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));

            var messages = new List<InfobipOmniFailoverMessage>();

            var callbackData = activity.Entities?
                .SingleOrDefault(x =>
                    x.Type == InfobipEntityType.CallbackData)?
                .Properties.ToInfobipCallbackDataJson();

            if (activity.Recipient?.Id == null)
                throw new ValidationException("Activity must have a Recipient Id");

            var destinations = new[] { new InfobipDestination { To = new InfobipTo { PhoneNumber = activity.Recipient.Id } } };

            switch (activity.ChannelId)
            {
                case InfobipChannel.Sms:
                    {
                        var smsMessage = InfobipOmniSmsMessageFactory.Create(activity);
                        messages.Add(new InfobipOmniFailoverMessage
                        {
                            Destinations = destinations,
                            ScenarioKey = options.InfobipSmsScenarioKey,
                            CallbackData = callbackData,
                            Sms = smsMessage
                        });
                        break;
                    }
                case InfobipChannel.WhatsApp:
                default:
                    {
                        var whatsAppMessages = InfobipOmniWhatsAppMessageFactory.Create(activity);
                        messages.AddRange(
                            whatsAppMessages.Select(
                                whatsAppMessage => new InfobipOmniFailoverMessage
                                {
                                    Destinations = destinations,
                                    ScenarioKey = options.InfobipWhatsAppScenarioKey,
                                    CallbackData = callbackData,
                                    WhatsApp = whatsAppMessage
                                }));

                        break;
                    }
            }

            return messages;
        }
    }
}
