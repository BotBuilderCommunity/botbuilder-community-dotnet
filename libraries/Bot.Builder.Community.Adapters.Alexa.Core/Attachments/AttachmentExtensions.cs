using Alexa.NET.Response;
using Bot.Builder.Community.Adapters.Shared;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Alexa.Core.Attachments
{
    public static class AttachmentExtensions
    {
        public static Attachment ToAttachment(this ICard card)
        {
            return SharedAttachmentHelper.CreateAttachment(card, AlexaAttachmentContentTypes.Card);
        }

        public static Attachment ToAttachment(this IDirective directive)
        {
            return SharedAttachmentHelper.CreateAttachment(directive, AlexaAttachmentContentTypes.Directive);
        }
    }
}
