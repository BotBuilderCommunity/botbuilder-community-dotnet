using System;
using System.Collections.Generic;
using Bot.Builder.Community.Adapters.Shared.Attachments;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Attachments
{
    public static class DefaultGoogleAttachmentConverter
    {
        public static AttachmentConverter CreateDefault() => new AttachmentConverter(new GoogleBusinessMessagesAttachmentConverter());
    }

    public class GoogleBusinessMessagesAttachmentConverter : AttachmentConverterBase
    {
        private readonly IReadOnlyDictionary<string, Action<Attachment>> _converters ;

        public GoogleBusinessMessagesAttachmentConverter()
        {
            _converters = new Dictionary<string, Action<Attachment>>
            {
                { GoogleAttachmentContentTypes.OpenUrlActionSuggestion, x => Convert<OpenUrlActionSuggestion>(x) },
                { GoogleAttachmentContentTypes.LiveAgentRequestSuggestion, x => Convert<LiveAgentRequestSuggestion>(x) },
                { GoogleAttachmentContentTypes.AuthenticationRequestSuggestion, x => Convert<AuthenticationRequestSuggestion>(x) },
                { GoogleAttachmentContentTypes.DialActionSuggestion, x => Convert<DialActionSuggestion>(x) },
                { GoogleAttachmentContentTypes.Image, x => Convert<Image>(x) },
                { GoogleAttachmentContentTypes.RichCard, x => Convert<RichCardContent>(x) },
            };
        }

        protected override string Name => nameof(GoogleBusinessMessagesAttachmentConverter);
        protected override IReadOnlyDictionary<string, Action<Attachment>> Converters => _converters;
    }
}
