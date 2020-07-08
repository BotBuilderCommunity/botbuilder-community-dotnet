namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model
{
    public class ActionsSdkResponse
    {
        public Session Session { get; set; }

        public Prompt Prompt { get; set; }

        public Scene Scene { get; set; }

        public User User { get; set; }

        public Home Home { get; set; }

        public Device Device { get; set; }

        public Expected Expected { get; set; }
    }
}
