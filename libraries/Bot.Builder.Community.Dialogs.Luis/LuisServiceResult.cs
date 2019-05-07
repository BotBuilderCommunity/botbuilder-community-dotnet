using Bot.Builder.Community.Dialogs.Luis.Models;

namespace Bot.Builder.Community.Dialogs.Luis
{
    /// <summary>
    /// Matches a LuisResult object with the best scored IntentRecommendation of the LuisResult
    /// and corresponding Luis service.
    /// </summary>
    public class LuisServiceResult
    {
        public LuisServiceResult(LuisResult result, IntentRecommendation intent, ILuisService service, ILuisOptions luisRequest = null)
        {
            this.Result = result;
            this.BestIntent = intent;
            this.LuisService = service;
            this.LuisRequest = luisRequest;
        }

        public LuisResult Result { get; }

        public IntentRecommendation BestIntent { get; }

        public ILuisService LuisService { get; }

        public ILuisOptions LuisRequest { get; }
    }
}
