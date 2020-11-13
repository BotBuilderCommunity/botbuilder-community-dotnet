using Bot.Builder.Community.Adapters.Infobip.Viber.Models;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Infobip.Viber
{
    public static class InfobipViberExtensions
    {
        public static void AddInfobipViberMessage(this Activity activity, InfobipOmniViberMessage message)
        {
            var attachment = new Attachment
            {
                ContentType = InfobipViberMessageContentTypes.Message,
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
