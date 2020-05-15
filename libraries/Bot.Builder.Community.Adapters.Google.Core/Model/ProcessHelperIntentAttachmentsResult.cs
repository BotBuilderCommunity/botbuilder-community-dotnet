using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;

namespace Bot.Builder.Community.Adapters.Google.Core.Model
{
    public class ProcessHelperIntentAttachmentsResult
    {
        public SystemIntent Intent { get; set; }
        public bool AllowAdditionalInputPrompt { get; set; }
    }
}