using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class ExpectedInput
    {
        public SystemIntent[] PossibleIntents { get; set; }
        public InputPrompt InputPrompt { get; set; }
    }
}