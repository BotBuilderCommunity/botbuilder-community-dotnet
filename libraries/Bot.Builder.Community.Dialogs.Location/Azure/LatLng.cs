using Newtonsoft.Json;
using System;

namespace Bot.Builder.Community.Dialogs.Location.Azure
{
    [Serializable]
    public class LatLng
    {
        [JsonProperty(PropertyName = "lat")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "lon")]
        public double Longitude { get; set; }
    }
}
