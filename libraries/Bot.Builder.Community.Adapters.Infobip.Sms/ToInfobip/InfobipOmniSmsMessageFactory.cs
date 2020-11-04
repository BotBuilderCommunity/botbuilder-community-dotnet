using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Sms.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.Sms.ToInfobip
{
    public class InfobipOmniSmsMessageFactory
    {
        public static InfobipOmniSmsMessage Create(Activity activity)
        {
            if (activity.Text == null) return null;
            var message = new InfobipOmniSmsMessage { Text = activity.Text };

            var smsOptions = activity.Entities?
                .SingleOrDefault(x =>
                    x.Type == InfobipSmsEntityType.SmsMessageOptions)?
                .Properties.ToInfobipOmniSmsMessageOptions();

            if (smsOptions != default)
                message.SetOptions(smsOptions);

            return message;
        }
    }
}