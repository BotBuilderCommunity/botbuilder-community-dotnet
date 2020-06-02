using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Samples.Infobip.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //RECEIVE MESSAGES 

            //MEDIA
            string mediaType;
            var attachment = turnContext.Activity.Attachments.First();
            if (attachment.ContentType.Contains("image"))
                mediaType = "IMAGE";
            else if (attachment.ContentType.Contains("audio"))
                mediaType = "AUDIO";
            else if (attachment.ContentType.Contains("video"))
                mediaType = "VIDEO";
            else if (attachment.ContentType.Contains("application")) 
                mediaType = "DOCUMENT";

            //DOWNLOAD MEDIA
            await attachment.DownloadContent();
            var content = attachment.Content;

            //LOCATION
            var locationMessage = turnContext.Activity.Entities.First().GetAs<GeoCoordinates>();

            //TEXT
            var textMessage = turnContext.Activity.Text;

            //SEND MESSAGES
            IMessageActivity activity;

            //TEXT
            var message = "Some text with *bold*, _italic_, ~strike through~ and ```code``` formatting";
            activity = MessageFactory.Text(message);

            //LOCATION
            activity.Entities.Add(new GeoCoordinates
            {
                Latitude = 12.3456789,
                Longitude = 23.456789,
                Name = "caption"
            });
            //or
            activity.Entities.Add(new Place
            {
                Address = "Address",
                Geo = new GeoCoordinates { Latitude = 12.3456789, Longitude = 23.456789, Name = "caption" }
            });

            //MEDIA
            var mediaMessage = new Attachment
            {
                ContentType = "image/png",
                ContentUrl = "https://cdn-www.infobip.com/wp-content/uploads/2019/09/05100710/rebranding-blog-banner-v3.png"
            };
            activity = MessageFactory.Attachment(mediaMessage);

            //TEMPLATE
            var templateMessage = new InfobipWhatsAppTemplateMessage
            {
                TemplateNamespace = "template_namespace",
                TemplateData = new[] { "one", "two" },
                TemplateName = "template_name",
                Language = "en",
                MediaTemplateData = new InfobipWhatsAppMediaTemplateData
                {
                    MediaTemplateHeader = new InfobipWhatsAppMediaTemplateHeader
                    {
                        DocumentFilename = "Test file name"
                    },
                    MediaTemplateBody = new InfobipWhatsAppMediaTemplateBody
                    {
                        Placeholders = new[] { "three", "four" }
                    }
                }
            };
            activity.Attachments.Add(new InfobipAttachment(templateMessage));

            await turnContext.SendActivityAsync(activity, cancellationToken);
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            //RECEIVE

            switch (turnContext.Activity.Name)
            {
                case InfobipReportTypes.DELIVERY:
                    var deliveryReport = activity.ChannelData as InfobipIncomingResult;
                    //CALLBACK DATA
                    var callbackData = activity.Entities
                        .First(x => x.Type == InfobipConstants.InfobipCallbackDataEntityType);
                    var sentData = callbackData.GetAs<Dictionary<string, string>>();
                    break;
                case InfobipReportTypes.SEEN:
                    var seenReport = activity.ChannelData as InfobipIncomingResult;
                    break;
                default:
                    return;
            }

            await turnContext.SendActivityAsync(
                $"Event received. Name: {turnContext.Activity.Name}. Value: {turnContext.Activity.ChannelData}", cancellationToken:cancellationToken).ConfigureAwait(false);
        }
    }
}
