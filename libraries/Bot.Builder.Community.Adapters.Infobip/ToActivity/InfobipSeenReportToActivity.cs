using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.ToActivity
{
    public class InfobipSeenReportToActivity : InfobipBaseConverter
    {
        public static Activity Convert(InfobipIncomingResult result)
        {
            var activity = ConvertToEvent(result);
            activity.Name = InfobipReportTypes.SEEN;
            activity.Timestamp = result.SeenAt;
            activity.ChannelId = InfobipChannel.WhatsApp;
            activity.From = new ChannelAccount { Id = result.From };

            return activity;

        }
    }
}