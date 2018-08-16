namespace Bot.Builder.Community.Dialogs.Location.Azure
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// An address.
    /// </summary>
    [Serializable]
    public class SearchAddress
    {
        /// <summary>
        /// The street number of an address.
        /// </summary>
        [JsonProperty(PropertyName = "streetNumber")]
        public string StreetNumber { get; set; }

        /// <summary>
        ///The name of the street.
        /// </summary>
        [JsonProperty(PropertyName = "streetName")]
        public string StreetName { get; set; }

        /// <summary>
        /// A string specifying the subdivision name in the country or region for an address.
        /// This element is typically treated as the first order administrative subdivision, 
        /// but in some cases it is the second, third, or fourth order subdivision in a country, dependency, or region.
        /// </summary>
        [JsonProperty(PropertyName = "countrySubdivisionName")]
        public string CountrySubdivisionName { get; set; }

        /// <summary>
        /// A string specifying the subdivision name in the country or region for an address. 
        /// This element is used when there is another level of subdivision information for a location, such as the county.
        /// </summary>
        [JsonProperty(PropertyName = "countrySecondarySubdivision")]
        public string CountrySecondarySubdivision { get; set; }

        /// <summary>
        /// A string specifying the country or region name of an address.
        /// </summary>
        [JsonProperty(PropertyName = "countryCodeISO3")]
        public string CountryCodeISO3 { get; set; }

        /// <summary>
        /// A string specifying the complete address. This address may not include the country or region.
        /// </summary>
        [JsonProperty(PropertyName = "freeformAddress")]
        public string FreeformAddress { get; set; }

        /// <summary>
        /// A string specifying the populated place for the address. 
        /// This typically refers to a city, but may refer to a suburb or a neighborhood in certain countries.
        /// </summary>
        [JsonProperty(PropertyName = "municipalitySubdivision")]
        public string MunicipalitySubdivision { get; set; }

        /// <summary>
        /// A string specifying the post code, postal code, or ZIP Code of an address.
        /// </summary>
        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        internal Bing.Address ToBingAddress()
        {
            string addressLine = string.Empty;

            if (!string.IsNullOrEmpty(StreetNumber) && !string.IsNullOrEmpty(StreetName))
            {
                addressLine = StreetNumber + " " + StreetName;
            }
            else if (!string.IsNullOrEmpty(StreetName))
            {
                addressLine = StreetName;
            }

            string locality = MunicipalitySubdivision;

            if(!string.IsNullOrEmpty(locality) && locality.Contains(","))
            {
                locality = locality.Split(',')[0];
            }

            string postCode = PostalCode;

            if (!string.IsNullOrEmpty(postCode) && postCode.Contains(","))
            {
                postCode = postCode.Split(',')[0];
            }

            return new Bing.Address()
            {
                AddressLine = addressLine,
                AdminDistrict = CountrySubdivisionName,
                AdminDistrict2 = CountrySecondarySubdivision,
                FormattedAddress = FreeformAddress,
                CountryRegion = CountryCodeISO3,
                Locality = locality,
                PostalCode = postCode
            };
        }
    }
}
