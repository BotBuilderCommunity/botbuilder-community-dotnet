using Bot.Builder.Community.Recognizers.Fuzzy;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.Middleware.Multilingual.AzureTranslateService
{
    public interface ITranslateService
    {
        LanguageInformation GetLanguageInformation(string text);
        Task<FuzzyMatch> IsLanguageAvailable(string text);
        Task<string> TranslateText(string sourceLanguage, string targetLanguage, string text);

        string DefaultLanguageCode { get; }
        double ScoreThreshold { get; }
        bool IsMultilingualEnabled { get;}
    }
}
