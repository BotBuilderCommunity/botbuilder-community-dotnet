using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.Viber.ToActivity
{
    public class InfobipViberToActivity: InfobipBaseConverter
    {
        public static Activity Convert(InfobipViberIncomingResult result)
        {
            var activity = ConvertToMessage(result);
            activity.ChannelId = InfobipViberConstants.ChannelName;
            activity.Text = result.Message.Text;
            activity.TextFormat = TextFormatTypes.Plain;

            return activity;
        }
    }
}
