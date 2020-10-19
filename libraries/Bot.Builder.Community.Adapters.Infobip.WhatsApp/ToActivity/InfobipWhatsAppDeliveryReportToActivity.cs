using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp.ToActivity
{
    public class InfobipWhatsAppDeliveryReportToActivity : InfobipBaseConverter
    {
        public static Activity Convert(InfobipWhatsAppIncomingResult result, InfobipWhatsAppAdapterOptions whatsAppAdapterOptions)
        {
            var activity = CreateBaseDeliveryReportActivity(result);

            activity.ChannelId = InfobipWhatsAppConstants.ChannelName;
            activity.From = new ChannelAccount { Id = whatsAppAdapterOptions.InfobipWhatsAppNumber };

            return activity;
        }
    }
}