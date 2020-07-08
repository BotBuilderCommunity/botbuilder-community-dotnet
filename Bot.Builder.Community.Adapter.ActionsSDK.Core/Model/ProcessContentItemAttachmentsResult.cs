using Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model
{
    public class ProcessContentItemAttachmentsResult
    {
        public ContentItem Item { get; set; }
        public bool AllowAdditionalInputPrompt { get; set; }
    }
}