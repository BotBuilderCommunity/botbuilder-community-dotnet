using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.Middleware.SentimentAnalysis
{
    internal static class SentimentExtensions
    {
        internal async static Task<string> Sentiment(this string text, string apiKey, string endpointUrl)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "0.0";
            }

            if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(endpointUrl))
            {
                return await GetSentimentWithCognitiveService(text, apiKey, endpointUrl);
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

        private static async Task<string> GetSentimentWithCognitiveService(string text, string apiKey, string endpointUrl)
        {
            ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                // Replace 'westus' with the correct region for your Text Analytics subscription
                Endpoint = endpointUrl
            };

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Extract the language
            var result = await client.DetectLanguageAsync(text, "us");
            var language = result.DetectedLanguages?[0].Iso6391Name;

            // Get the sentiment
            var sentimentResult = await client.SentimentAsync(text, language);

            return sentimentResult?.Score?.ToString("#.#");
        }
    }
}
