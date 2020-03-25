using System;
using System.Collections.Generic;
using System.Text;
using Alexa.NET.Response;
using Microsoft.Bot.Schema;

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
                Content = card,
                ContentType = contentType,
            };
        }
    }
}
