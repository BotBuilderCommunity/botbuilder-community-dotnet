using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Alexa.Core.Attachments
{
    public class PermissionConsentRequestAttachment : Attachment
    {
        public PermissionConsentRequestAttachment(List<string> permissions)
        {
            Permissions = permissions;
        }

        public List<string> Permissions { get; set; }
    }
}
