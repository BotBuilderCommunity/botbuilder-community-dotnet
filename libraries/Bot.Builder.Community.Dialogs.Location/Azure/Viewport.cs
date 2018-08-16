using Newtonsoft.Json;
using System;

namespace Bot.Builder.Community.Dialogs.Location.Azure
{
    [Serializable]
    public class Viewport
    {
        [JsonProperty(PropertyName = "topLeftPoint")]
        public LatLng TopLeftPoint { get; set; }

        [JsonProperty(PropertyName = "btmRightPoint")]
        public LatLng BtmRightPoint { get; set; }
    }
}
