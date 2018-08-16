using System.Linq;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Dialogs.Location
{
    /// <summary>
    /// Extensions for <see cref="Place"/>
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the postal address.
        /// </summary>
        /// <param name="place">The place.</param>
        /// <returns>The <see cref="PostalAddress"/> if available, null otherwise.</returns>
        public static PostalAddress GetPostalAddress(this Place place)
        {
            if (place.Address != null)
            {
                return (PostalAddress)place.Address;
            }

            return null;
        }

        /// <summary>
        /// Gets the geo coordinates.
        /// </summary>
        /// <param name="place">The place.</param>
        /// <returns>The <see cref="GeoCoordinates"/> if available, null otherwise.</returns>
        public static GeoCoordinates GetGeoCoordinates(this Place place)
        {
            if (place.Geo != null)
            {
                return (GeoCoordinates)place.Geo;
            }

            return null;
        }

        internal static string GetFormattedAddress(this Bing.Location location, string separator)
        {
            if (location?.Address == null)
            {
                return null;
            }

            return string.Join(separator, new[]
            {
                location.Address.AddressLine,
                location.Address.Locality,
                location.Address.AdminDistrict,
                location.Address.PostalCode,
                location.Address.CountryRegion
            }.Where(x => !string.IsNullOrEmpty(x)));
        }
    }
}
