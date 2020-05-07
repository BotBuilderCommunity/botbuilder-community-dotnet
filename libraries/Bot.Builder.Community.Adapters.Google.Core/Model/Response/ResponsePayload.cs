using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;

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
        public SystemIntent SystemIntent { get; set; }
        public string UserStorage { get; set; }
    }
}