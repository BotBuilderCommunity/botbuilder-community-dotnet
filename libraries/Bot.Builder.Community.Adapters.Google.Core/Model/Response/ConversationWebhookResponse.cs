using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class ConversationWebhookResponse
    {
        public string ConversationToken { get; set; }
        public object UserStorage { get; set; }
        public bool? ResetUserStorage { get; set; }
        public bool ExpectUserResponse { get; set; }
        public ExpectedInput[] ExpectedInputs { get; set; }
        public FinalResponse FinalResponse { get; set; }
        public CustomPushMessage CustomPushMessage { get; set; }
        public bool IsInSandbox { get; set; }
        public SystemIntent SystemIntent { get; set; }
    }
}