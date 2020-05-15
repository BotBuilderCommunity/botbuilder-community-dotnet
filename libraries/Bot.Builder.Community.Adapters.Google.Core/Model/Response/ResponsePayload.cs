using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class ResponsePayload
    {
        public PayloadContent Google { get; set; }
    }

    public class PayloadContent
    {
        public bool ExpectUserResponse { get; set; }
        public RichResponse RichResponse { get; set; }
        public DialogFlowSystemIntent SystemIntent { get; set; }
        public object UserStorage { get; set; }
    }
}