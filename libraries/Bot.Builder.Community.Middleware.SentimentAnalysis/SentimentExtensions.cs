using System;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;

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
                // Replace 'westus' with the correct region for your Text Analytics subscription
                Endpoint = "https://westus.api.cognitive.microsoft.com"
            };

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Extract the language
            var result = await client.DetectLanguageAsync(text);
            var language = result.DetectedLanguages?[0].Iso6391Name;

            // Get the sentiment
            var sentimentResult = await client.SentimentAsync(text, language);

            return sentimentResult?.Score?.ToString("#.#");
        }
    }
}
