using System;
using System.Collections.Generic;
using Bot.Builder.Community.Dialogs.Location.Azure;
using Bot.Builder.Community.Dialogs.Location.Bing;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Dialogs.Location
{
    public class LocationCardBuilder : ILocationCardBuilder
    {
        private readonly string apiKey;
        private readonly LocationResourceManager resourceManager;
        private readonly bool useAzureMaps = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationCardBuilder"/> class.
        /// </summary>
        /// <param name="apiKey">The geo spatial API key.</param>
        public LocationCardBuilder(string apiKey, LocationResourceManager resourceManager)
        {
            //SetField.NotNull(out this.apiKey, nameof(apiKey), apiKey);
            //SetField.NotNull(out this.resourceManager, nameof(resourceManager), resourceManager);
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            if (resourceManager == null)
                throw new ArgumentNullException(nameof(resourceManager));

            this.apiKey = apiKey;
            this.resourceManager = resourceManager;

            if (!string.IsNullOrEmpty(this.apiKey) && this.apiKey.Length > 60)
            {
                useAzureMaps = false;
            }
        }

        /// <summary>
        /// Creates locations hero cards.
        /// </summary>
        /// <param name="locations">List of the locations.</param>
        /// <param name="alwaysShowNumericPrefix">Indicates whether a list containing exactly one location should have a '1.' prefix in its label.</param>
        /// <param name="locationNames">List of strings that can be used as names or labels for the locations.</param>
        /// <returns>The locations card as a list.</returns>
        public IEnumerable<HeroCard> CreateHeroCards(IList<Bing.Location> locations, bool alwaysShowNumericPrefix = false, IList<string> locationNames = null)
        {
            var cards = new List<HeroCard>();

            int i = 1;

            foreach (var location in locations)
            {
                string nameString = locationNames == null ? string.Empty : $"{locationNames[i - 1]}: ";
                string locationString = $"{nameString}{location.GetFormattedAddress(resourceManager.AddressSeparator)}";
                string address = alwaysShowNumericPrefix || locations.Count > 1 ? $"{i}. {locationString}" : locationString;

                var heroCard = new HeroCard
                {
                    Subtitle = address
                };

                if (location.Point != null)
                {
                    IGeoSpatialService geoService;

                    if (useAzureMaps)
                    {
                        geoService = new AzureMapsSpatialService(apiKey);
                    }
                    else
                    {
                        geoService = new BingGeoSpatialService(apiKey);
                    }

                    var image =
                        new CardImage(
                            url: geoService.GetLocationMapImageUrl(location, i));

                    heroCard.Images = new[] { image };
                }

                cards.Add(heroCard);

                i++;
            }

            return cards;
        }
    }
}
