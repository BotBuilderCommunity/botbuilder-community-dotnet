using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Microsoft.Bot.Schema;
using Microsoft.Rest;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.ToInfobip
{
    public class ToViberInfobipConverter
    {
        /// <summary>
        /// Converts a Bot Framework activity to a Infobip OMNI failover message ready for the OMNI API.
        /// </summary>
        /// <param name="activity">The activity to be converted to Infobip OMNI failover message.</param>
        /// <param name="scenarioKey">The Infobip scenario key.</param>
        /// <returns>The resulting messages.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is null.</exception>
        /// <exception cref="ValidationException"><paramref name="activity"/> is null.</exception>
        public static List<InfobipViberOmniFailoverMessage> Convert(Activity activity, string scenarioKey)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));

            var messages = new List<InfobipViberOmniFailoverMessage>();

            var callbackData = activity.Entities?
                .SingleOrDefault(x =>
                    x.Type == InfobipEntityType.CallbackData)?
                .Properties.ToInfobipCallbackDataJson();

            if (activity.Recipient?.Id == null)
                throw new ValidationException("Activity must have a Recipient Id");

            var destinations = new[] { new InfobipDestination { To = new InfobipTo { PhoneNumber = activity.Recipient.Id } } };

            var viberMessages = InfobipOmniViberMessageFactory.Create(activity);
            messages.AddRange(
                viberMessages.Select(
                    viberMessage => new InfobipViberOmniFailoverMessage
                    {
                        Destinations = destinations,
                        ScenarioKey = scenarioKey,
                        CallbackData = callbackData,
                        Viber = viberMessage
                    }));

            return messages;
        }
    }
}
