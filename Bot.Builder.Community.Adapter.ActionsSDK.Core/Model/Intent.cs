using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model
{
    public class Intent
    {
        public string Name { get; set; }

        public Dictionary<string, JObject> Params { get; set; }

        public string Query { get; set; }
    }
}