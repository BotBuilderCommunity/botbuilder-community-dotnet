namespace Bot.Builder.Community.Adapters.ActionsSDK
{
    public class ActionsSdkAdapterOptions
    {
        public bool ShouldEndSessionByDefault { get; set; } = true;

        public string ActionInvocationName { get; set; }

        public string ActionProjectId { get; set; }
    }
}
