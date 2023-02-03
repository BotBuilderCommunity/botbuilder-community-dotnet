using Bot.Builder.Community.Recognizers.Fuzzy;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using Bot.Builder.Community.Components.Middleware.Multilingual.AzureTranslateService;

namespace Bot.Builder.Community.Components.Middleware.Multilingual.Middleware
{
    public partial class MultilingualMiddleware
    {
        private const string Property = "turn.Multilingual";

        private async Task<string> FindLanguage(ITurnContext turnContext)
        {
            var userLanguage = _translateService.DefaultLanguageCode;

            var defaultLanguageInformation = new LanguageInformation()
            {
                Language = "English",
                LanguageCode = _translateService.DefaultLanguageCode,
            };
            
            var score = await GetLanguageScore(turnContext.Activity.Text);
            var isMultilingual = false; 

            if (score?.Score > _translateService.ScoreThreshold)
            {
                defaultLanguageInformation = _translateService.GetLanguageInformation(score.Choice);
                userLanguage = defaultLanguageInformation.LanguageCode;
                isMultilingual = true;
            }

            if (score == null) return userLanguage;
            
            var conversationExpire = new
            {
                LanguageChoice = score.Choice,
                LanguageScore = score.Score,
                UserText = turnContext.Activity.Text,
                LanguageCode = defaultLanguageInformation.LanguageCode,
                ConversationLanguage = defaultLanguageInformation.Language,
                IsEnabled = isMultilingual,
                SupportedListUrl = "https://learn.microsoft.com/en-us/azure/cognitive-services/translator/language-support#translation"
            };

            var languageInfo = JsonConvert.SerializeObject(conversationExpire);

            ObjectPath.SetPathValue(turnContext.TurnState, Property, JObject.Parse(languageInfo));

            return userLanguage;
        }

        private async Task<FuzzyMatch> GetLanguageScore(string language)
        {
            var score = await _translateService.IsLanguageAvailable(language) ?? new FuzzyMatch
            {
                Score = 0,
                Choice = string.Empty
            };

            return score;
        }

        private async Task<string> GetCurrentLanguage(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await _languageStateProperty.GetAsync(turnContext, () => string.Empty, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task TranslateMessageActivityAsync(IMessageActivity activity, string targetLocale)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                activity.Text = await _translateService.TranslateText(_translateService.DefaultLanguageCode, targetLocale, activity.Text);
            }
        }
    }
}