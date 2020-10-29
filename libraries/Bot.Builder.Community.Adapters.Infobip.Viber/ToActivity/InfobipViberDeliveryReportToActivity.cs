using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.ToActivity
{
    public class InfobipViberDeliveryReportToActivity: InfobipBaseConverter
    {
        public static Activity Convert(InfobipViberIncomingResult result, string viberSender)
        {
            var activity = CreateBaseDeliveryReportActivity(result);
            activity.ChannelId = InfobipViberConstants.ChannelName;
            activity.From = new ChannelAccount { Id = viberSender };
            return activity;
        }
    }
}
