namespace Bot.Builder.Community.Adapter.ActionsSDK.Core
{
    public class ActionsSdkRequestMapperOptions
    {
        public string ChannelId { get; set; } = "google";
        public string ServiceUrl { get; set; } = null;
        public bool ShouldEndSessionByDefault { get; set; } = true;
        public string ActionInvocationName { get; set; } = null;
    }
}
