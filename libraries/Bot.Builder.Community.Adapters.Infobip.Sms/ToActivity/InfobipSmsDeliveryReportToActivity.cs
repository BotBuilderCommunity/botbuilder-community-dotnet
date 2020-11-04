using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Sms.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.Sms.ToActivity
{
    public class InfobipSmsDeliveryReportToActivity : InfobipBaseConverter
    {
        public static Activity Convert(InfobipSmsIncomingResult result, string smsNumber)
        {
            var activity = CreateBaseDeliveryReportActivity(result);
            activity.ChannelId = InfobipSmsConstants.ChannelName;
            activity.From = new ChannelAccount { Id = smsNumber };
            return activity;
        }
    }
}