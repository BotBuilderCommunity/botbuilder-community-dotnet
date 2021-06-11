namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model
{
    public class OpenUrlActionSuggestion : Suggestion
    {
        public OpenUrlSuggestedActionContent Action { get; set; }
    }

    public class OpenUrlSuggestedActionContent : SuggestedAction
    {
        public OpenUrlAction OpenUrlAction { get; set; }
    }
}