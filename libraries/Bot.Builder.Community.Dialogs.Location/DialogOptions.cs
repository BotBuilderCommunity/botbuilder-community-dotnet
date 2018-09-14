using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Builder.Community.Dialogs.Location
{
    public class SelectLocationDialogOptions
    {
        public List<Bing.Location> Locations { get; set; }
    }

    public class CompleteMissingFieldsDialogOptions
    {
        public Bing.Location Location { get; set; }
    }

    public class ConfirmDeleteFavoriteDialogOptions
    {
        public FavoriteLocation Location { get; set; }
    }

    public class AddToFavoritesDialogOptions
    {
        public Bing.Location Location { get; set; }
    }
}
