namespace Bot.Builder.Community.Dialogs.Location.Bing
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a geo point.
    /// </summary>
    [Serializable]
    public class GeocodePoint
    {
        /// <summary>
        /// List of the coordinates.
        /// </summary>
        [JsonProperty(PropertyName = "coordinates")]
        public List<double> Coordinates { get; set; }

        ///// <summary>
        ///// The method that was used to compute the geocode point.
        ///// </summary>
        //[JsonProperty(PropertyName = "calculationMethod")]
        //public string CalculationMethod { get; set; }

        ///// <summary>
        ///// The best use for the geocode point.
        ///// </summary>
        //[JsonProperty(PropertyName = "usageTypes")]
        //public List<string> UsageTypes { get; set; }

        /// <summary>
        /// Returns whether point has geo coordinates or not.
        /// </summary>
        [JsonIgnore]
        public bool HasCoordinates => Coordinates != null && Coordinates.Count == 2;
    }
}