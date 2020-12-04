using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Sms.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.Sms.ToActivity
{
    // handle SMS messages
    // - https://dev-old.infobip.com/receive-sms/forward-method
    public class InfobipSmsToActivity : InfobipBaseConverter
    {
        public static Activity Convert(InfobipSmsIncomingResult result)
        {
            var activity = ConvertToMessage(result);
            activity.ChannelId = InfobipSmsConstants.ChannelName;
            activity.Text = result.CleanText;
            activity.TextFormat = TextFormatTypes.Plain;

            return activity;
        }
    }
}