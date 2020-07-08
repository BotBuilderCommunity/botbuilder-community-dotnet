using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model
{
    public class Scene
    {
        public string Name { get; set; }

        public string SlotFillingStatus { get; set; }

        public Dictionary<string, JObject> Slots { get; set; }

        public NextScene Next { get; set; }
    }
}