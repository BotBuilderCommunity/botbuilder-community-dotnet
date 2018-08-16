namespace Bot.Builder.Community.Dialogs.Location.Bing
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the location returned from the Bing Geo Spatial API:
    /// https://msdn.microsoft.com/en-us/library/ff701725.aspx
    /// </summary>
    [Serializable]
    public class Location
    {
        ///// <summary>
        ///// The location type.
        ///// </summary>
        //[JsonProperty(PropertyName = "__type")]
        //public string LocationType { get; set; }

        /// <summary>
        /// A geographic area that contains the location. A bounding box contains SouthLatitude, 
        /// WestLongitude, NorthLatitude, and EastLongitude values in units of degrees.
        /// </summary>
        [JsonProperty(PropertyName = "bbox")]
        public List<double> BoundaryBox { get; set; }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The latitude and longitude coordinates of the location.
        /// </summary>
        [JsonProperty(PropertyName = "point")]
        public GeocodePoint Point { get; set; }

        /// <summary>
        /// The postal address for the location. An address can contain AddressLine, Neighborhood, 
        /// Locality, AdminDistrict, AdminDistrict2, CountryRegion, CountryRegionIso2, PostalCode, 
        /// FormattedAddress, and Landmark fields.
        /// </summary>
        [JsonProperty(PropertyName = "address")]
        public Address Address { get; set; }

        ///// <summary>
        ///// The level of confidence that the geocoded location result is a match.
        ///// Use this value with the match code to determine for more complete information about the match.
        ///// </summary>
        //[JsonProperty(PropertyName = "confidence")]
        //public string Confidence { get; set; }

        /// <summary>
        /// The classification of the geographic entity returned, such as Address. 
        /// For a list of entity types, see Location and Area Types.
        /// </summary>
        [JsonProperty(PropertyName = "entityType")]
        public string EntityType { get; set; }

        /// <summary>
        /// A collection of geocoded points that differ in how they were calculated and their suggested use.
        /// For a description of the points in this collection, see the Geocode Point Fields section below.
        /// </summary>
        [JsonProperty(PropertyName = "geocodePoints")]
        public List<GeocodePoint> GeocodePoints { get; set; }

        ///// <summary>
        ///// One or more match code values that represent the geocoding level for each location in the response.
        ///// </summary>
        //[JsonProperty(PropertyName = "matchCodes")]
        //public List<string> MatchCodes { get; set; }
    }
}