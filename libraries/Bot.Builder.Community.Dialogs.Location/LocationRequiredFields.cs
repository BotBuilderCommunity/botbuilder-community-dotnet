using System;

namespace Bot.Builder.Community.Dialogs.Location
{
    [Flags]
    public enum LocationRequiredFields
    {
        /// <summary>
        /// No required fields.
        /// </summary>
        None = 0,

        /// <summary>
        /// The street address
        /// </summary>
        /// <example>One Microsoft Way</example>
        StreetAddress = 1,

        /// <summary>
        /// The locality
        /// </summary>
        /// <example>Redmond</example>
        Locality = 2,

        /// <summary>
        /// The region
        /// </summary>
        /// <example>WA</example>
        Region = 4,

        /// <summary>
        /// The country
        /// </summary>
        /// <example>United States</example>
        Country = 8,

        /// <summary>
        /// The postal code
        /// </summary>
        /// <example>98052</example>
        PostalCode = 16
    }
}
