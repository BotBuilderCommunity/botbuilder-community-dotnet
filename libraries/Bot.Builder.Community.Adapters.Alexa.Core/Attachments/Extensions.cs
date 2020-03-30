using Alexa.NET.Response;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Alexa.Core.Attachments
{
    public static class Extensions
    {
        public static Attachment ToAttachment(this ICard card)
        {
            return CreateAttachment(card, AlexaAttachmentContentTypes.Card);
        }

        public static Attachment ToAttachment(this IDirective directive)
        {
            return CreateAttachment(directive, AlexaAttachmentContentTypes.Directive);
        }

        private static Attachment CreateAttachment<T>(T card, string contentType)
        {
            return new Attachment
            {
                Content = JObject.FromObject(card, new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore }),
                ContentType = contentType,
            };
        }
    }
}
