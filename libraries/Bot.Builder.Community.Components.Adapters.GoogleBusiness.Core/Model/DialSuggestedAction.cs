namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class DialActionSuggestion : Suggestion
    {
        public DialSuggestedActionContent Action { get; set; }
    }

    public class DialSuggestedActionContent : SuggestedAction
    {
        public DialAction DialAction { get; set; }
    }
}