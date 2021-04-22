using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.Handoff.LivePerson.Models
{
    public class LivePersonCredentialsProvider : ILivePersonCredentialsProvider
    {
        public LivePersonCredentialsProvider(IConfiguration configuration)
        {
            LpAccount = configuration["LivePersonAccount"];
            LpAppId = configuration["LivePersonClientId"];
            LpAppSecret = configuration["LivePersonClientSecret"];
            MsAppId = configuration["MicrosoftAppId"];
        }

        public string LpAccount { get; }

        public string LpAppId { get; }

        public string LpAppSecret { get; }

        public string MsAppId { get; }
    }
}
