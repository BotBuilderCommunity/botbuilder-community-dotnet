using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model
{
    public class Intent
    {
        public string Name { get; set; }

        public JObject Params { get; set; }

        public string Query { get; set; }
    }
}