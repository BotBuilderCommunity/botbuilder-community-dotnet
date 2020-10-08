using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.ToActivity
{
    // handle SMS messages
    // - https://dev-old.infobip.com/receive-sms/forward-method
    public class InfobipSmsToActivity : InfobipBaseConverter
    {
        public static Activity Convert(InfobipIncomingResult result)
        {
            var activity = ConvertToMessage(result);
            activity.ChannelId = InfobipChannel.Sms;
            activity.Text = result.CleanText;
            activity.TextFormat = TextFormatTypes.Plain;

            return activity;
        }
    }
}