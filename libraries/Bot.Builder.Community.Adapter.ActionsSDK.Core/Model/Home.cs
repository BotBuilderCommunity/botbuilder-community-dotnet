using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Model
{
    public class Home
    {
        public Dictionary<string, JObject> Params { get; set; }
    }
}