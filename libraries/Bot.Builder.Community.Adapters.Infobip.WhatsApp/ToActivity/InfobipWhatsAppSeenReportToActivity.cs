using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp.ToActivity
{
    public class InfobipWhatsAppSeenReportToActivity : InfobipBaseConverter
    {
        public static Activity Convert(InfobipWhatsAppIncomingResult result)
        {
            var activity = ConvertToEvent(result);
            activity.Name = InfobipReportTypes.SEEN;
            activity.Timestamp = result.SeenAt;
            activity.ChannelId = InfobipWhatsAppConstants.ChannelName;
            activity.From = new ChannelAccount { Id = result.From };

            return activity;

        }
    }
}