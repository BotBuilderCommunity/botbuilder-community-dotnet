using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model
{
    public class User
    {
        public string Locale { get; set; }

        public Dictionary<string, JObject> Params { get; set; }

        public string AccountLinkingStatus { get; set; }

        public string VerificationStatus { get; set; }

        public string LastSeenTime { get; set; }

        public JObject Engagement { get; set; }

        public List<JObject> PackageEntitlements { get; set; }
    }
}