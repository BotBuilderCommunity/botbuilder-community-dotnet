using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model
{
    public class User
    {
        public string Locale { get; set; }

        public JObject Params { get; set; }

        public string AccountLinkingStatus { get; set; }

        public string VerificationStatus { get; set; }

        public string LastSeenTime { get; set; }

        public JObject Engagement { get; set; }

        public List<JObject> PackageEntitlements { get; set; }

        public string AccessToken { get; set; }
    }
}