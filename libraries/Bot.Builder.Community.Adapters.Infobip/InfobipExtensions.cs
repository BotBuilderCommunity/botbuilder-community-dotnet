using System.Collections.Generic;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Infobip.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Infobip
{
    public static class InfobipExtensions
    {
        public static async Task DownloadContent(this Attachment self)
        {
            var attachment = await InfobipClient.GetAttachmentAsync(self.ContentUrl).ConfigureAwait(false);
            self.Content = attachment?.Content;
            self.ContentType = attachment?.ContentType;
        }

        public static void AddInfobipOmniSmsMessageOptions(this Activity activity, InfobipOmniSmsMessageOptions options)
        {
            var serializer = new JsonSerializer();
            var entity = new Entity
            {
                Type = InfobipEntityType.SmsMessageOptions,
                Properties = JObject.FromObject(options, serializer)
            };

            AddEntity(activity, entity);
        }

        public static void AddInfobipCallbackData(this Activity activity, IDictionary<string, string> callbackData)
        {
            var serializer = new JsonSerializer();
            var entity = new Entity
            {
                Type = InfobipEntityType.CallbackData,
                Properties = JObject.FromObject(callbackData, serializer)
            };
            AddEntity(activity, entity);
        }

        public static void AddInfobipWhatsAppTemplateMessage(this Activity activity, InfobipWhatsAppTemplateMessage message)
        {
            var attachment = new Attachment
            {
                ContentType = InfobipAttachmentContentTypes.WhatsAppMessageTemplate,
                Content = message
            };
            AddAttachment(activity, attachment);
        }

        private static void AddEntity(Activity activity, Entity entity)
        {
            if (activity.Entities == null) activity.Entities = new List<Entity>();
            activity.Entities.Add(entity);
        }

        private static void AddAttachment(Activity activity, Attachment attachment)
        {
            if (activity.Attachments == null) activity.Attachments = new List<Attachment>();
            activity.Attachments.Add(attachment);
        }
    }
}
