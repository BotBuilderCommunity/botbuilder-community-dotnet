using System.Reflection;
using System.Resources;
using Bot.Builder.Community.Dialogs.Location.Resources;

namespace Bot.Builder.Community.Dialogs.Location
{
    public class LocationResourceManager
    {
        private readonly ResourceManager resourceManager;

        /// <summary>
        /// The <see cref="AddressSeparator"/> resource string.
        /// </summary>
        public virtual string AddressSeparator => GetResource(nameof(Strings.AddressSeparator));

        /// <summary>
        /// The <see cref="AddToFavoritesAsk"/> resource string.
        /// </summary>
        public virtual string AddToFavoritesAsk => GetResource(nameof(Strings.AddToFavoritesAsk));

        /// <summary>
        /// The <see cref="AddToFavoritesRetry"/> resource string.
        /// </summary>
        public virtual string AddToFavoritesRetry => GetResource(nameof(Strings.AddToFavoritesRetry));

        /// <summary>
        /// The <see cref="AskForEmptyAddressTemplate"/> resource string.
        /// </summary>
        public virtual string AskForEmptyAddressTemplate => GetResource(nameof(Strings.AskForEmptyAddressTemplate));

        /// <summary>
        /// The <see cref="AskForPrefix"/> resource string.
        /// </summary>
        public virtual string AskForPrefix => GetResource(nameof(Strings.AskForPrefix));

        /// <summary>
        /// The <see cref="AskForTemplate"/> resource string.
        /// </summary>
        public virtual string AskForTemplate => GetResource(nameof(Strings.AskForTemplate));

        /// <summary>
        /// The <see cref="CancelCommand"/> resource string.
        /// </summary>
        public virtual string CancelCommand => GetResource(nameof(Strings.CancelCommand));

        /// <summary>
        /// The <see cref="CancelPrompt"/> resource string.
        /// </summary>
        public virtual string CancelPrompt => GetResource(nameof(Strings.CancelPrompt));

        /// <summary>
        /// The <see cref="ConfirmationAsk"/> resource string.
        /// </summary>
        public virtual string ConfirmationAsk => GetResource(nameof(Strings.ConfirmationAsk));

        /// <summary>
        /// The <see cref="ConfirmationInvalidResponse"/> resource string.
        /// </summary>
        public virtual string ConfirmationInvalidResponse => GetResource(nameof(Strings.ConfirmationInvalidResponse));

        /// <summary>
        /// The <see cref="Country"/> resource string.
        /// </summary>
        public virtual string Country => GetResource(nameof(Strings.Country));

        /// <summary>
        /// The <see cref="DeleteCommand"/> resource string.
        /// </summary>
        public virtual string DeleteCommand => GetResource(nameof(Strings.DeleteCommand));

        /// <summary>
        /// The <see cref="DeleteFavoriteAbortion"/> resource string.
        /// </summary>
        public virtual string DeleteFavoriteAbortion => GetResource(nameof(Strings.DeleteFavoriteAbortion));

        /// <summary>
        /// The <see cref="DeleteFavoriteConfirmationAsk"/> resource string.
        /// </summary>
        public virtual string DeleteFavoriteConfirmationAsk => GetResource(nameof(Strings.DeleteFavoriteConfirmationAsk));

        /// <summary>
        /// The <see cref="DialogStartBranchAsk"/> resource string.
        /// </summary>
        public virtual string DialogStartBranchAsk => GetResource(nameof(Strings.DialogStartBranchAsk));

        /// <summary>
        /// The <see cref="DuplicateFavoriteNameResponse"/> resource string.
        /// </summary>
        public virtual string DuplicateFavoriteNameResponse => GetResource(nameof(Strings.DuplicateFavoriteNameResponse));

        /// <summary>
        /// The <see cref="EditCommand"/> resource string.
        /// </summary>
        public virtual string EditCommand => GetResource(nameof(Strings.EditCommand));

        /// <summary>
        /// The <see cref="EditFavoritePrompt"/> resource string.
        /// </summary>
        public virtual string EditFavoritePrompt => GetResource(nameof(Strings.EditFavoritePrompt));

        /// <summary>
        /// The <see cref="EnterNewFavoriteLocationName"/> resource string.
        /// </summary>
        public virtual string EnterNewFavoriteLocationName => GetResource(nameof(Strings.EnterNewFavoriteLocationName));

        /// <summary>
        /// The <see cref="FavoriteAddedConfirmation"/> resource string.
        /// </summary>
        public virtual string FavoriteAddedConfirmation => GetResource(nameof(Strings.FavoriteAddedConfirmation));

        /// <summary>
        /// The <see cref="FavoriteDeletedConfirmation"/> resource string.
        /// </summary>
        public virtual string FavoriteDeletedConfirmation => GetResource(nameof(Strings.FavoriteDeletedConfirmation));

        /// <summary>
        /// The <see cref="FavoriteEdittedConfirmation"/> resource string.
        /// </summary>
        public virtual string FavoriteEdittedConfirmation => GetResource(nameof(Strings.FavoriteEdittedConfirmation));

        /// <summary>
        /// The <see cref="FavoriteLocations"/> resource string.
        /// </summary>
        public virtual string FavoriteLocations => GetResource(nameof(Strings.FavoriteLocations));

        /// <summary>
        /// The <see cref="HelpCommand"/> resource string.
        /// </summary>
        public virtual string HelpCommand => GetResource(nameof(Strings.HelpCommand));

        /// <summary>
        /// The <see cref="HelpMessage"/> resource string.
        /// </summary>
        public virtual string HelpMessage => GetResource(nameof(Strings.HelpMessage));

        /// <summary>
        /// The <see cref="InvalidFavoriteLocationSelection"/> resource string.
        /// </summary>
        public virtual string InvalidFavoriteLocationSelection => GetResource(nameof(Strings.InvalidFavoriteLocationSelection));

        /// <summary>
        /// The <see cref="InvalidFavoriteNameResponse"/> resource string.
        /// </summary>
        public virtual string InvalidFavoriteNameResponse => GetResource(nameof(Strings.InvalidFavoriteNameResponse));


        /// <summary>
        /// The <see cref="InvalidLocationResponse"/> resource string.
        /// </summary>
        public virtual string InvalidLocationResponse => GetResource(nameof(Strings.InvalidLocationResponse));

        /// <summary>
        /// The <see cref="InvalidLocationResponseFacebook"/> resource string.
        /// </summary>
        public virtual string InvalidLocationResponseFacebook => GetResource(nameof(Strings.InvalidLocationResponseFacebook));

        /// <summary>
        /// The <see cref="InvalidStartBranchResponse"/> resource string.
        /// </summary>
        public virtual string InvalidStartBranchResponse => GetResource(nameof(Strings.InvalidStartBranchResponse));

        /// <summary>
        /// The <see cref="LocationNotFound"/> resource string.
        /// </summary>
        public virtual string LocationNotFound => GetResource(nameof(Strings.LocationNotFound));

        /// <summary>
        /// The <see cref="Locality"/> resource string.
        /// </summary>
        public virtual string Locality => GetResource(nameof(Strings.Locality));

        /// <summary>
        /// The <see cref="MultipleResultsFound"/> resource string.
        /// </summary>
        public virtual string MultipleResultsFound => GetResource(nameof(Strings.MultipleResultsFound));

        /// <summary>
        /// The <see cref="NoFavoriteLocationsFound"/> resource string.
        /// </summary>
        public virtual string NoFavoriteLocationsFound => GetResource(nameof(Strings.NoFavoriteLocationsFound));

        /// <summary>
        /// The <see cref="OtherComand"/> resource string.
        /// </summary>
        public virtual string OtherComand => GetResource(nameof(Strings.OtherComand));

        /// <summary>
        /// The <see cref="OtherLocation"/> resource string.
        /// </summary>
        public virtual string OtherLocation => GetResource(nameof(Strings.OtherLocation));

        /// <summary>
        /// The <see cref="PostalCode"/> resource string.
        /// </summary>
        public virtual string PostalCode => GetResource(nameof(Strings.PostalCode));

        /// <summary>
        /// The <see cref="Region"/> resource string.
        /// </summary>
        public virtual string Region => GetResource(nameof(Strings.Region));

        /// <summary>
        /// The <see cref="ResetCommand"/> resource string.
        /// </summary>
        public virtual string ResetCommand => GetResource(nameof(Strings.ResetCommand));

        /// <summary>
        /// The <see cref="ResetPrompt"/> resource string.
        /// </summary>
        public virtual string ResetPrompt => GetResource(nameof(Strings.ResetPrompt));

        /// <summary>
        /// The <see cref="SelectFavoriteLocationPrompt"/> resource string.
        /// </summary>
        public virtual string SelectFavoriteLocationPrompt => GetResource(nameof(Strings.SelectFavoriteLocationPrompt));

        /// <summary>
        /// The <see cref="SelectLocation"/> resource string.
        /// </summary>
        public virtual string SelectLocation => GetResource(nameof(Strings.SelectLocation));

        /// <summary>
        /// The <see cref="SingleResultFound"/> resource string.
        /// </summary>
        public virtual string SingleResultFound => GetResource(nameof(Strings.SingleResultFound));

        /// <summary>
        /// The <see cref="StreetAddress"/> resource string.
        /// </summary>
        public virtual string StreetAddress => GetResource(nameof(Strings.StreetAddress));

        /// <summary>
        /// The <see cref="TitleSuffix"/> resource string.
        /// </summary>
        public virtual string TitleSuffix => GetResource(nameof(Strings.TitleSuffix));

        /// <summary>
        /// The <see cref="TitleSuffixFacebook"/> resource string.
        /// </summary>
        public virtual string TitleSuffixFacebook => GetResource(nameof(Strings.TitleSuffixFacebook));

        /// <summary>
        /// Default constructor. Initializes strings using GaryPretty.Bot.Builder.Location assembly resources.
        /// </summary>
        public LocationResourceManager() :
            this(null, null)
        {
        }

        internal LocationResourceManager(Assembly resourceAssembly = null, string resourceName = null)
        {
            if (resourceAssembly == null || resourceName == null)
            {
                resourceAssembly = typeof(LocationDialog).Assembly;
                resourceName = typeof(Strings).FullName;
            }

            resourceManager = new ResourceManager(resourceName, resourceAssembly);
        }

        private string GetResource(string name)
        {
            return resourceManager.GetString(name) ??
                   Strings.ResourceManager.GetString(name);
        }
    }
}
