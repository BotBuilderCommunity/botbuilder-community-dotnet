using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Dialogs.Location.Dialogs
{
    internal class FavoritesManager : IFavoritesManager
    {
        private readonly IStatePropertyAccessor<List<FavoriteLocation>> _favoriteLocations;

        public FavoritesManager(IStatePropertyAccessor<List<FavoriteLocation>> favoriteLocations)
        {
            _favoriteLocations = favoriteLocations;
        }

        private const string FavoritesKey = "locationFavorites";
        private const int MaxFavoriteCount = 5;

        public async Task<bool> MaxCapacityReached(ITurnContext context)
        {
            var favorites = await GetFavorites(context);
            return favorites.Count >= MaxFavoriteCount;
        }

        public async Task<bool> IsFavorite(ITurnContext context, Bing.Location location)
        {
            var favorites = await GetFavorites(context);
            return favorites.Any(favoriteLocation => AreEqual(location, favoriteLocation.Location));
        }

        public async Task<bool> IsFavoriteLocationName(ITurnContext context, string name)
        {
            var favorites = await GetFavorites(context);
            return favorites.Any(favoriteLocation => StringComparer.OrdinalIgnoreCase.Equals(name, favoriteLocation.Name));
        }

        public async Task<FavoriteLocation> GetFavoriteByIndex(ITurnContext context, int index)
        {
            var favorites = await GetFavorites(context);

            if (index >= 0 && index < favorites.Count)
            {
                return favorites[index];
            }

            return null;
        }

        public async Task<FavoriteLocation> GetFavoriteByName(ITurnContext context, string name)
        {
            var favorites = await GetFavorites(context);
            return favorites.FirstOrDefault(favoriteLocation => StringComparer.OrdinalIgnoreCase.Equals(name, favoriteLocation.Name));
        }

        public async Task Add(ITurnContext context, FavoriteLocation value)
        {
            var favorites = await GetFavorites(context);

            if (favorites.Count >= MaxFavoriteCount)
            {
                throw new InvalidOperationException("The max allowed number of favorite locations has already been reached.");
            }

            favorites.Add(value);

            await _favoriteLocations.SetAsync(context, favorites);
        }

        public async Task Delete(ITurnContext context, FavoriteLocation value)
        {
            var favorites = await GetFavorites(context);
            var newFavorites = new List<FavoriteLocation>();

            foreach (var favoriteItem in favorites)
            {
                if (!AreEqual(favoriteItem.Location, value.Location))
                {
                    newFavorites.Add(favoriteItem);
                }
            }

            await _favoriteLocations.SetAsync(context, newFavorites);
        }

        public async Task Update(ITurnContext context, FavoriteLocation currentValue, FavoriteLocation newValue)
        {
            var favorites = await GetFavorites(context);
            var newFavorites = new List<FavoriteLocation>();

            foreach (var item in favorites)
            {
                if (AreEqual(item.Location, currentValue.Location))
                {
                    newFavorites.Add(newValue);
                }
                else
                {
                    newFavorites.Add(item);
                }
            }

            await _favoriteLocations.SetAsync(context, newFavorites);
        }

        public async Task<List<FavoriteLocation>> GetFavorites(ITurnContext context)
        {
            return await _favoriteLocations.GetAsync(context, () => { return new List<FavoriteLocation>(); });
        }

        private static bool AreEqual(Bing.Location x, Bing.Location y)
        {
            // Other attributes of a location such as its Confidence, BoundaryBox, etc
            // should not be considered as distinguishing factors.
            // On the other hand, attributes of a location that are shown to the users
            // are what distinguishes one location from another. 
            return x.GetFormattedAddress(",") == y.GetFormattedAddress(",");
        }
    }
}
