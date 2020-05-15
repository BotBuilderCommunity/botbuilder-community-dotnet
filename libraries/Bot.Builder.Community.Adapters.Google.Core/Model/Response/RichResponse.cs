namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class RichResponse
    {
        public ResponseItem[] Items { get; set; }
        public Suggestion[] Suggestions { get; set; }
        public LinkOutSuggestion LinkOutSuggestion { get; set; }
    }
}