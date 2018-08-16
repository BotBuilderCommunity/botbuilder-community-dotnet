namespace Bot.Builder.Community.Dialogs.Location.Bing
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [Serializable]
    internal class LocationApiResponse
    {
        [JsonProperty(PropertyName = "authenticationResultCode")]
        public string AuthenticationResultCode { get; set; }

        [JsonProperty(PropertyName = "brandLogoUri")]
        public string BrandLogoUri { get; set; }

        [JsonProperty(PropertyName = "copyright")]
        public string Copyright { get; set; }

        [JsonProperty(PropertyName = "resourceSets")]
        public List<LocationSet> LocationSets { get; set; }

        [JsonProperty(PropertyName = "statusCode")]
        public int SatusCode { get; set; }

        [JsonProperty(PropertyName = "statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty(PropertyName = "traceId")]
        public string TraceId { get; set; }
    }
}