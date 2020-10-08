using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.ToActivity
{
    public class InfobipDeliveryReportToActivity : InfobipBaseConverter
    {
        public static Activity Convert(InfobipIncomingResult result, InfobipAdapterOptions adapterOptions)
        {
            var activity = CreateBaseDeliveryReportActivity(result);
            var channel = GetInfobipChannelFromDeliveryReportMessage(result.Channel);

            switch (channel)
            {
                case InfobipChannel.Sms:
                    activity.ChannelId = InfobipChannel.Sms;
                    activity.From = new ChannelAccount { Id = adapterOptions.InfobipSmsNumber };
                    break;
                case InfobipChannel.WhatsApp:
                    activity.ChannelId = InfobipChannel.WhatsApp;
                    activity.From = new ChannelAccount { Id = adapterOptions.InfobipWhatsAppNumber };
                    break;
            }

            return activity;
        }

        private static string GetInfobipChannelFromDeliveryReportMessage(string responseChannel)
        {
            if (string.IsNullOrWhiteSpace(responseChannel))
                return null;
            return $"infobip-{responseChannel.ToLower()}";
        }

        private static Activity CreateBaseDeliveryReportActivity(InfobipIncomingResult response)
        {
            var activity = ConvertToEvent(response);
            activity.Name = InfobipReportTypes.DELIVERY;
            activity.Timestamp = response.DoneAt;
            return activity;
        }
    }
}