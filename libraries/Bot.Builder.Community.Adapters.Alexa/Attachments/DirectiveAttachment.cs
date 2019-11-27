using System;
using System.Collections.Generic;
using System.Text;
using Alexa.NET.Response;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Alexa.Attachments
{
    public class DirectiveAttachment : Attachment
    {
        public DirectiveAttachment(IDirective directive)
        {
            this.Directive = directive;
        }

        public IDirective Directive { get; set; }
    }
}
