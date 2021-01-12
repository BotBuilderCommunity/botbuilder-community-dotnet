using Bot.Builder.Community.Adapters.Infobip.WhatsApp.Models;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Adapters.Infobip.WhatsApp
{
    public static class InfobipWhatsAppExtensions
    {
        public static async Task DownloadContent(this Attachment self)
        {
            var attachment = await InfobipWhatsAppClient.GetAttachmentAsync(self.ContentUrl).ConfigureAwait(false);
            self.Content = attachment?.Content;
            self.ContentType = attachment?.ContentType;
        }

        public static void AddInfobipWhatsAppTemplateMessage(this Activity activity, InfobipWhatsAppTemplateMessage message)
        {
            var attachment = new Attachment
            {
                ContentType = InfobipWhatsAppAttachmentContentTypes.WhatsAppMessageTemplate,
                Content = message
            };
            AddAttachment(activity, attachment);
        }

        private static void AddAttachment(Activity activity, Attachment attachment)
        {
            if (activity.Attachments == null) activity.Attachments = new List<Attachment>();
            activity.Attachments.Add(attachment);
        }
    }
}
