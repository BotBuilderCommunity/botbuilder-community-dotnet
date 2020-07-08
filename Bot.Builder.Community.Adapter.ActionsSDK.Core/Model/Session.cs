using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model
{
    public class Session
    {
        public string Id { get; set; }

        public Dictionary<string, JObject> Params { get; set; }

        public List<TypeOverride> TypeOverrides { get; set; }

        public string LanguageCode { get; set; }
    }
}