using Microsoft.Azure.CognitiveServices.Language.SpellCheck;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.SpellCheck
{
    internal static class SpellCheckExtensions
    {
        internal static async Task<string> SpellCheck(this string text, string apiKey, string countryCode = "US", string market = "en-US")
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var client = new SpellCheckClient(new ApiKeyServiceClientCredentials(apiKey));
            var spellCheckResult = await client.SpellCheckerAsync(text, setLang: market, market: market, countryCode: countryCode, mode:"spell");

            foreach (var flaggedToken in spellCheckResult.FlaggedTokens)
            {
                text = text.Replace(flaggedToken.Token, flaggedToken.Suggestions.FirstOrDefault().Suggestion);
            }
            return text;
        }
    }
}
