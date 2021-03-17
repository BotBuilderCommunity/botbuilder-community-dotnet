using System.ComponentModel.DataAnnotations;

namespace Bot.Builder.Community.Adapters.ActionsSDK
{
    public class ActionsSdkAdapterOptions
    {
        public bool ShouldEndSessionByDefault { get; set; } = true;

        [Required]
        public string ActionInvocationName { get; set; }

        [Required]
        public string ActionProjectId { get; set; }
    }
}
