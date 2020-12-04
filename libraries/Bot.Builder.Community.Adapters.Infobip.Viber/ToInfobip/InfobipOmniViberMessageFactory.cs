using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.ToInfobip
{
    public class InfobipOmniViberMessageFactory
    {
        //Any combination of text, image or buttons is allowed.
        //The only constraint when sending the button that the buttonUrl and buttonText are mandatory!
        public static IList<InfobipOmniViberMessage> Create(Activity activity)
        {
            var messages = new List<InfobipOmniViberMessage>();

            if (!string.IsNullOrWhiteSpace(activity.Text))
                messages.Add(new InfobipOmniViberMessage { Text = activity.Text });

            var viberMessages =
                activity.Attachments?.Where(x => x.ContentType == InfobipViberMessageContentTypes.Message);
            if (viberMessages == null) return messages;

            foreach (var viberMessage in viberMessages)
            {
                if (!(viberMessage.Content is InfobipOmniViberMessage message)) continue;
                if (string.IsNullOrWhiteSpace(message.ButtonText) ^
                    string.IsNullOrWhiteSpace(message.ButtonUrl))
                    throw new Exception("When sending the button that the buttonUrl and buttonText are mandatory!");
                messages.Add(message);

            }

            return messages;
        }
    }
}
