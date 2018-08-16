using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Dialogs.Location
{
    interface IFavoritesManager
    {
        Task<bool> MaxCapacityReached(ITurnContext context);

        Task<bool> IsFavorite(ITurnContext context, Bing.Location location);

        Task<bool> IsFavoriteLocationName(ITurnContext context, string name);

        Task<FavoriteLocation> GetFavoriteByIndex(ITurnContext context, int index);

        Task<FavoriteLocation> GetFavoriteByName(ITurnContext context, string name);

        Task<List<FavoriteLocation>> GetFavorites(ITurnContext context);

        Task Add(ITurnContext context, FavoriteLocation value);

        Task Delete(ITurnContext context, FavoriteLocation value);

        Task Update(ITurnContext context, FavoriteLocation currentValue, FavoriteLocation newValue);
    }
}
