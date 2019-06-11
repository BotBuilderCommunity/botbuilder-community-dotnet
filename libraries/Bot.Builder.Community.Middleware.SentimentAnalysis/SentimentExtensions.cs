using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.SentimentAnalysis
{
    internal static class SentimentExtensions
    {
        internal static async Task<string> Sentiment(this string text, string apiKey)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "0.0";
            }

            if (!string.IsNullOrEmpty(apiKey))
            {
                return await GetSentimentWithCognitiveService(text, apiKey);
            }
            else
            {
                return GetSentimentWithMLModel(text);
            }
        }

        private static string GetSentimentWithMLModel(string text)
        {
            var prediction = SentimentAnalyzer.Sentiments.Predict(text);
            return prediction.Prediction.ToString(); // true in case of Postive, false in case of Negative
        }

        private static async Task<string> GetSentimentWithCognitiveService(string text, string apiKey)
        {
            ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = "https://westus.api.cognitive.microsoft.com"
            }; //Replace 'westus' with the correct region for your Text Analytics subscription

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Extract the language
            var result = await client.DetectLanguageAsync(false, new LanguageBatchInput(new List<LanguageInput>() { new LanguageInput(id: "1", text: text) }));
            var language = result.Documents?[0].DetectedLanguages?[0].Iso6391Name;

            // Get the sentiment
            var sentimentResult = await client.SentimentAsync(false,
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput(language, "0", text),
                        }));

            return sentimentResult.Documents?[0].Score?.ToString("#.#");
        }
    }
}
