using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.Luis.Models;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Dialogs.Luis
{
    /// <summary>
    /// Standard implementation of ILuisService against actual LUIS service.
    /// </summary>
    [Serializable]
    public sealed class LuisService : ILuisService
    {
        private readonly ILuisModel model;

        /// <summary>
        /// Construct the LUIS service using the model information.
        /// </summary>
        /// <param name="model">The LUIS model information.</param>
        public LuisService(ILuisModel model)
        {
            SetField.NotNull(out this.model, nameof(model), model);
        }

        public ILuisModel LuisModel => model;

        public static void Fix(LuisResult result)
        {
            // fix up Luis result for backward compatibility
            // v2 api is not returning list of intents if verbose query parameter
            // is not set. This will move IntentRecommendation in TopScoringIntent
            // to list of Intents.
            if (result.Intents == null || result.Intents.Count == 0)
            {
                if (result.TopScoringIntent != null)
                {
                    result.Intents = new List<IntentRecommendation> { result.TopScoringIntent };
                }
            }
        }

        public LuisRequest ModifyRequest(LuisRequest request)
        {
            return model.ModifyRequest(request);
        }

        Uri ILuisService.BuildUri(LuisRequest luisRequest)
        {
            return luisRequest.BuildUri(this.model);
        }

        public void ApplyThreshold(LuisResult result)
        {
            if (result.TopScoringIntent.Score > model.Threshold)
            {
                return;
            }

            result.TopScoringIntent.Intent = "None";
            result.TopScoringIntent.Score = 1.0d;
        }

        async Task<LuisResult> ILuisService.QueryAsync(Uri uri, CancellationToken token)
        {
            string json;
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token))
            {
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync();
            }

            try
            {
                var result = JsonConvert.DeserializeObject<LuisResult>(json);
                Fix(result);
                ApplyThreshold(result);
                return result;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the LUIS response.", ex);
            }
        }
    }
}