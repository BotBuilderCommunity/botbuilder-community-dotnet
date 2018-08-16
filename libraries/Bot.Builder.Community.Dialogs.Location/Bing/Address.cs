namespace Bot.Builder.Community.Dialogs.Location.Bing
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// An address can contain the following fields: address line, locality, 
    /// neighborhood, admin district, admin district 2, formatted address, postal code and country or region.
    /// </summary>
    [Serializable]
    public class Address
    {
        /// <summary>
        /// The official street line of an address relative to the area, as specified by the Locality, or PostalCode, properties. 
        /// Typical use of this element would be to provide a street address or any official address.
        /// </summary>
        [JsonProperty(PropertyName = "addressLine")]
        public string AddressLine { get; set; }

        /// <summary>
        /// A string specifying the subdivision name in the country or region for an address.
        /// This element is typically treated as the first order administrative subdivision, 
        /// but in some cases it is the second, third, or fourth order subdivision in a country, dependency, or region.
        /// </summary>
        [JsonProperty(PropertyName = "adminDistrict")]
        public string AdminDistrict { get; set; }

        /// <summary>
        /// A string specifying the subdivision name in the country or region for an address. 
        /// This element is used when there is another level of subdivision information for a location, such as the county.
        /// </summary>
        [JsonProperty(PropertyName = "adminDistrict2")]
        public string AdminDistrict2 { get; set; }

        /// <summary>
        /// A string specifying the country or region name of an address.
        /// </summary>
        [JsonProperty(PropertyName = "countryRegion")]
        public string CountryRegion { get; set; }

        /// <summary>
        /// A string specifying the complete address. This address may not include the country or region.
        /// </summary>
        [JsonProperty(PropertyName = "formattedAddress")]
        public string FormattedAddress { get; set; }

        /// <summary>
        /// A string specifying the populated place for the address. 
        /// This typically refers to a city, but may refer to a suburb or a neighborhood in certain countries.
        /// </summary>
        [JsonProperty(PropertyName = "locality")]
        public string Locality { get; set; }

        /// <summary>
        /// A string specifying the post code, postal code, or ZIP Code of an address.
        /// </summary>
        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }
    }
}