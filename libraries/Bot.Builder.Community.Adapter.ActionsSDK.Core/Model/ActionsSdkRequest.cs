namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model
{
    public class ActionsSdkRequest
    {
        public Handler Handler { get; set; }

        public Intent Intent { get; set; }

        public Scene Scene { get; set; }

        public Session Session { get; set; }

        public User User { get; set; }

        public Home Home { get; set; }

        public Device Device { get; set; }

        public Context Context { get; set; }
    }
}
