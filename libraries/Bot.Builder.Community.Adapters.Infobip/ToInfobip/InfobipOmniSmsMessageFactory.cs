using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.ToInfobip
{
    public class InfobipOmniSmsMessageFactory
    {
        public static InfobipOmniSmsMessage Create(Activity activity)
        {
            var message = new InfobipOmniSmsMessage { Text = activity.Text };

            var smsOptions = activity.Entities?
                .SingleOrDefault(x =>
                    x.Type == InfobipEntityType.SmsMessageOptions)?
                .Properties.ToInfobipOmniSmsMessageOptions();

            if (smsOptions != default)
                message.SetOptions(smsOptions);

            return message;
        }
    }
}