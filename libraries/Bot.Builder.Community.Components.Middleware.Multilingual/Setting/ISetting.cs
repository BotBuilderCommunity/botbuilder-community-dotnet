namespace Bot.Builder.Community.Components.Middleware.Multilingual.AzureTranslateService
{
    public interface ISetting
    {
        string Key { get; set; }
        string Endpoint { get; set; }
        string Location { get; set; }
        
        string DefaultLanguageCode { get; set; }
        double ScoreThreshold { get; set; }
        bool IsMultilingualEnabled { get; set; }
    }
}