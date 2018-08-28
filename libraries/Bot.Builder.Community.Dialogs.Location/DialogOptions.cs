using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Builder.Community.Dialogs.Location
{
    public class SelectLocationDialogOptions : DialogOptions
    {
        public List<Bing.Location> Locations { get; set; }
    }

    public class CompleteMissingFieldsDialogOptions : DialogOptions
    {
        public Bing.Location Location { get; set; }
    }

    public class ConfirmDeleteFavoriteDialogOptions : DialogOptions
    {
        public FavoriteLocation Location { get; set; }
    }

    public class AddToFavoritesDialogOptions : DialogOptions
    {
        public Bing.Location Location { get; set; }
    }
}
