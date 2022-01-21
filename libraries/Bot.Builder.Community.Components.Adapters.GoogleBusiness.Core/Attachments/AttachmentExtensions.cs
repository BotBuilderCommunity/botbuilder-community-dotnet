using Bot.Builder.Community.Adapters.Shared.Attachments;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Attachments
{
    public static class AttachmentExtensions
    {
        public static Attachment ToAttachment(this Suggestion responseItem)
        {
            switch (responseItem)
            {
                case OpenUrlActionSuggestion openUrlSuggestedAction:
                    return SharedAttachmentHelper.CreateAttachment(openUrlSuggestedAction, GoogleAttachmentContentTypes.OpenUrlActionSuggestion);
                case DialActionSuggestion dialSuggestedAction:
                    return SharedAttachmentHelper.CreateAttachment(dialSuggestedAction, GoogleAttachmentContentTypes.DialActionSuggestion);
                case LiveAgentRequestSuggestion liveAgentRequestSuggestion:
                    return SharedAttachmentHelper.CreateAttachment(liveAgentRequestSuggestion, GoogleAttachmentContentTypes.LiveAgentRequestSuggestion);
                case AuthenticationRequestSuggestion authenticationRequestSuggestion:
                    return SharedAttachmentHelper.CreateAttachment(authenticationRequestSuggestion, GoogleAttachmentContentTypes.AuthenticationRequestSuggestion);
                default:
                    return null;
            }
        }

        public static Attachment ToAttachment(this RichCardContent richCardContent)
        {
            return SharedAttachmentHelper.CreateAttachment(richCardContent, GoogleAttachmentContentTypes.RichCard);
        }

        public static Attachment ToAttachment(this Image image)
        {
            return SharedAttachmentHelper.CreateAttachment(image, GoogleAttachmentContentTypes.Image);
        }
    }
}
