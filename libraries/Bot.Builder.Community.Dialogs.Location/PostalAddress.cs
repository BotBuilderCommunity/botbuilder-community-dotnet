namespace Bot.Builder.Community.Dialogs.Location
{
    public class PostalAddress
    {
        /// <summary>
        /// Gets or sets the formatted address.
        /// </summary>
        /// <example>One Microsoft Way, Redmond, WA, United States (98052)</example>
        public string FormattedAddress { get; set; }

        /// <summary>
        /// Gets or sets the country
        /// </summary>
        /// <example>United States</example>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the locality
        /// </summary>
        /// <example>Redmond</example>
        public string Locality { get; set; }

        /// <summary>
        /// Gets or sets the region
        /// </summary>
        /// <example>WA</example>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the postal code
        /// </summary>
        /// <example>98052</example>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the street address
        /// </summary>
        /// <example>One Microsoft Way</example>
        public string StreetAddress { get; set; }
    }
}
