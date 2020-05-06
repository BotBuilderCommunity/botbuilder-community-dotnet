using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Infobip.Models
{
    public class InfobipAttachment: Attachment
    {
        public InfobipAttachment(InfobipWhatsAppTemplateMessage template)
        {
            ContentType = InfobipConstants.WhatsAppMessageTemplateContentType;
            Content = template;
        }
    }
}
