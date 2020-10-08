using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.ToActivity
{
    // handle any type of WA message sent by subscriber, can be: TEXT, IMAGE, DOCUMENT, LOCATION, CONTACT, VIDEO
    // - https://dev-old.infobip.com/whatsapp-business-messaging/incoming-whatsapp-messages
    public class InfobipWhatsAppToActivity : InfobipBaseConverter
    {
        public static async Task<Activity> Convert(InfobipIncomingResult result, IInfobipClient infobipClient)
        {
            var activity = ConvertToMessage(result);
            activity.ChannelId = InfobipChannel.WhatsApp;

            if (result.Message.Type == InfobipIncomingMessageTypes.Text)
            {
                activity.Text = result.Message.Text;
                activity.TextFormat = TextFormatTypes.Plain;
            }
            else if (result.Message.Type == InfobipIncomingMessageTypes.Location)
            {
                activity.Entities.Add(new GeoCoordinates
                {
                    Latitude = result.Message.Latitude,
                    Longitude = result.Message.Longitude
                });
            }
            else if (result.Message.IsMedia())
            {
                var contentType = await infobipClient.GetContentTypeAsync(result.Message.Url.AbsoluteUri).ConfigureAwait(false);
                activity.Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        ContentType = contentType,
                        ContentUrl = result.Message.Url.AbsoluteUri,
                        Name = result.Message.Caption
                    }
                };
            }
            else
                return null;

            return activity;
        }
    }
}