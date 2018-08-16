using Newtonsoft.Json;
using System;

namespace Bot.Builder.Community.Dialogs.Location.Azure
{
    [Serializable]
    public class PoiInfo
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
