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

            // Create a client.
            ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = "https://westus.api.cognitive.microsoft.com"
            }; //Replace 'westus' with the correct region for your Text Analytics subscription

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Extract the language
            var result = await client.DetectLanguageAsync(new BatchInput(new List<Input>() { new Input("1", text) }));
            var language = result.Documents?[0].DetectedLanguages?[0].Iso6391Name;

            // Get the sentiment
            var sentimentResult = await client.SentimentAsync(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput(language, "0", text),

                        }));

            return sentimentResult.Documents?[0].Score?.ToString("#.#");
        }
    }
}
