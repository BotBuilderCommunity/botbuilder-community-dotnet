using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.Location.Azure;
using Bot.Builder.Community.Dialogs.Location.Bing;
using Bot.Builder.Community.Dialogs.Location.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
using ConfirmPrompt = Microsoft.Bot.Builder.Dialogs.ConfirmPrompt;
using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

namespace Bot.Builder.Community.Dialogs.Location
{
    public class LocationDialog : ComponentDialog
    {
        private const int MaxLocationCount = 5;
        private IStatePropertyAccessor<List<FavoriteLocation>> _favoriteLocations;

        /// <summary>Contains the IDs for the other dialogs in the set.</summary>
        protected static class DialogIds
        {
            public const string FavoriteLocationRetrieverDialog = "FavoriteLocationRetrieverDialog";
            public const string LocationRetrieverRichDialog = "LocationRetrieverRichDialog";
            public const string AddToFavoritesDialog = "AddToFavoritesDialog";
            public const string SelectAndConfirmLocationDialog = "FindAndConfirmLocationDialog";
            public const string HeroStartCardDialog = "HeroStartCardDialog";
            public const string LocationRetrieverFacebookDialog = "LocationRetrieverFacebookDialog";
            public static string CompleteMissingRequiredFieldsDialog = "CompleteMissingRequiredFieldsDialog";
            public static string ConfirmDeleteFromFavoritesDialog = "ConfirmDeleteFromFavoritesDialog";
        }

        /// <summary>Contains the IDs for the prompts used by the dialogs.</summary>
        private static class PromptDialogIds
        {
            public const string Text = "textPrompt";
            public const string Choice = "choicePrompt";
            public const string Confirm = "confirmPrompt";
        }

        private static class StepContextKeys
        {
            public const string Locations = "locations";
            public const string SelectedLocation = "selectedLocation";
        }

        public LocationDialog(
            string apiKey,
            string prompt,
            BotState state,
            bool skipPrompt = false,
            bool useAzureMaps = true,
            LocationOptions options = LocationOptions.None,
            LocationRequiredFields requiredFields = LocationRequiredFields.None,
            LocationResourceManager resourceManager = null) : base("LocationDialog")
        {
            resourceManager = resourceManager ?? new LocationResourceManager();

            if (!options.HasFlag(LocationOptions.SkipFavorites) && state == null)
            {
                throw new ArgumentNullException(nameof(state),
                    "If LocationOptions.SkipFavorites is not used then BotState object must be " +
                    "provided to allow for storing / retrieval of favorites");
            }

            var favoritesManager = new FavoritesManager(_favoriteLocations);

            IGeoSpatialService geoSpatialService;
            if (useAzureMaps)
            {
                geoSpatialService = new AzureMapsSpatialService(apiKey);
            }
            else
            {
                geoSpatialService = new BingGeoSpatialService(apiKey);
            }

            InitialDialogId = "LocationDialog";

            AddDialog(new ChoicePrompt(PromptDialogIds.Choice));
            AddDialog(new TextPrompt(PromptDialogIds.Text));
            AddDialog(new ConfirmPrompt(PromptDialogIds.Confirm));

            AddDialog(new WaterfallDialog(InitialDialogId, new WaterfallStep[]
            {
                async (dc, stepContext) =>
                {
                    if (options.HasFlag(LocationOptions.SkipFavorites)
                        || !favoritesManager.GetFavorites(dc.Context).Result.Any())
                    {
                        var isFacebookChannel = StringComparer.OrdinalIgnoreCase.Equals(
                            dc.Context.Activity.ChannelId, "facebook");

                        if (options.HasFlag(LocationOptions.UseNativeControl) && isFacebookChannel)
                        {
                            await dc.BeginAsync(DialogIds.LocationRetrieverFacebookDialog);
                        }
                        else
                        {
                            await dc.BeginAsync(DialogIds.LocationRetrieverRichDialog);
                        }
                    }
                    else
                    {
                        await dc.BeginAsync(DialogIds.HeroStartCardDialog);
                    }

                    return EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    Bing.Location selectedLocation = (Bing.Location) stepContext.Result;
                    stepContext.Values[StepContextKeys.SelectedLocation] = selectedLocation;

                    if (options.HasFlag(LocationOptions.SkipFinalConfirmation))
                    {
                        await stepContext.NextAsync();
                    }
                    else
                    {
                        await dc.PromptAsync(PromptDialogIds.Confirm,
                            new PromptOptions()
                            {
                                Prompt = new Activity {
                                    Type = ActivityTypes.Message,
                                    Text = string.Format(resourceManager.ConfirmationAsk,
                                        selectedLocation.GetFormattedAddress(resourceManager.AddressSeparator)) },
                                RetryPrompt =  new Activity {
                                    Type = ActivityTypes.Message,
                                    Text = resourceManager.ConfirmationInvalidResponse }
                            });
                    }

                    return Dialog.EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    if (stepContext.Result is bool result && !result)
                    {
                        await dc.Context.SendActivityAsync(resourceManager.ResetPrompt);
                        await dc.ReplaceAsync(InitialDialogId);
                    }
                    else
                    {
                        if (!options.HasFlag(LocationOptions.SkipFavorites))
                        {
                            //await dc.BeginAsync(DialogIds.AddToFavoritesDialog, dc.ActiveDialog.State);
                        }
                    }

                    return Dialog.EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    Bing.Location selectedLocation = (Bing.Location) dc.ActiveDialog.State[StepContextKeys.SelectedLocation];
                    await dc.EndAsync(CreatePlace(selectedLocation));
                    return Dialog.EndOfTurn;
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.LocationRetrieverRichDialog, new WaterfallStep[]
            {
                async (dc, stepContext) =>
                {
                    if (!skipPrompt)
                    {
                        await dc.PromptAsync(PromptDialogIds.Text, new PromptOptions() {
                            Prompt = new Activity{ Type = ActivityTypes.Message, Text = prompt + resourceManager.TitleSuffix } });
                    }
                    return Dialog.EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    var locationQuery = ((string) stepContext.Result);
                    var locationSet = await geoSpatialService.GetLocationsByQueryAsync(locationQuery);
                    var foundLocations = locationSet?.Locations;

                    if (foundLocations == null || foundLocations.Count == 0)
                    {
                        await dc.Context.SendActivityAsync(resourceManager.LocationNotFound);
                        await dc.ReplaceAsync(DialogIds.LocationRetrieverRichDialog);
                    }
                    else
                    {
                        var locations = new List<Bing.Location>();
                        locations.AddRange(foundLocations.Take(MaxLocationCount));

                        await dc.BeginAsync(DialogIds.SelectAndConfirmLocationDialog,
                            new SelectLocationDialogOptions { Locations = locations });
                    }

                    return Dialog.EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    var selectedLocation = (Bing.Location) stepContext.Result;
                    await dc.EndAsync(selectedLocation);
                    return Dialog.EndOfTurn;
                }
}));

            AddDialog(new WaterfallDialog(DialogIds.SelectAndConfirmLocationDialog, new WaterfallStep[]
            {
                async (dc, stepContext) =>
                {
                    var locations = ((SelectLocationDialogOptions) stepContext.Options).Locations;
                    stepContext.Values[StepContextKeys.Locations] = locations;

                    var locationsCardReply = dc.Context.Activity.CreateReply();
                    var cardBuilder = new LocationCardBuilder(apiKey, resourceManager);
                    locationsCardReply.Attachments = cardBuilder.CreateHeroCards(locations)
                        .Select(C => C.ToAttachment()).ToList();
                    locationsCardReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    await dc.Context.SendActivityAsync(locationsCardReply);

                    if (locations.Count == 1)
                    {
                        await dc.PromptAsync(PromptDialogIds.Confirm,
                            new PromptOptions()
                            {
                                Prompt = new Activity { Type = ActivityTypes.Message, Text = resourceManager.SingleResultFound },
                                RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = resourceManager.ConfirmationInvalidResponse }
                            });
                    }
                    else
                    {
                        await dc.Context.SendActivityAsync(resourceManager.MultipleResultsFound);
                    }

                    return Dialog.EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    var locations = (List<Bing.Location>) stepContext.Values[StepContextKeys.Locations];

                    if (stepContext.Result is bool result)
                    {
                        if (result)
                        {
                            await TryReverseGeocodeAddress(locations.First(), options, geoSpatialService);

                            if (requiredFields == LocationRequiredFields.None)
                            {
                                await dc.EndAsync(locations.First());
                            }
                            else
                            {
                                await dc.ReplaceAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                                    new CompleteMissingFieldsDialogOptions { Location = locations.First() });
                            }
                        }
                        else
                        {
                            await dc.ReplaceAsync(DialogIds.LocationRetrieverRichDialog);
                        }
                    }
                    else
                    {
                        var message = dc.Context.Activity.Text;

                        if (int.TryParse(message, out var value) && value > 0 && value <= locations.Count)
                        {
                            await TryReverseGeocodeAddress(locations[value - 1], options, geoSpatialService);

                            if (requiredFields == LocationRequiredFields.None)
                            {
                                await dc.EndAsync(locations[value - 1]);
                            }
                            else
                            {
                                await dc.ReplaceAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                                                                    new CompleteMissingFieldsDialogOptions { Location = locations[value - 1] });
                            }
                        }
                        else if (StringComparer.OrdinalIgnoreCase.Equals(message, resourceManager.OtherComand))
                        {
                            await dc.ReplaceAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                                                                    new CompleteMissingFieldsDialogOptions { Location = new Bing.Location() });
                        }
                        else
                        {
                            await dc.Context.SendActivityAsync(resourceManager.InvalidLocationResponse);
                            await dc.ReplaceAsync(DialogIds.SelectAndConfirmLocationDialog,
                                new SelectLocationDialogOptions { Locations = (List<Bing.Location>)stepContext.Values[StepContextKeys.Locations] });
                        }
                    }

                    return Dialog.EndOfTurn;
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.CompleteMissingRequiredFieldsDialog, new WaterfallStep[]
            {
                    async (dc, stepContext) =>
                    {
                        var selectedLocation = ((CompleteMissingFieldsDialogOptions)stepContext.Options).Location;

                        stepContext.Values[StepContextKeys.SelectedLocation] = selectedLocation;

                        if (requiredFields.HasFlag(LocationRequiredFields.StreetAddress) &&
                            string.IsNullOrEmpty(selectedLocation.Address.AddressLine))
                        {
                            await PromptForRequiredField(dc, LocationRequiredFields.StreetAddress,
                                resourceManager.StreetAddress, resourceManager, selectedLocation);
                        }
                        else if (requiredFields.HasFlag(LocationRequiredFields.Locality) &&
                                 string.IsNullOrEmpty(selectedLocation.Address.Locality))
                        {
                            await PromptForRequiredField(dc, LocationRequiredFields.Locality, resourceManager.Locality,
                                resourceManager, selectedLocation);
                        }
                        else if (requiredFields.HasFlag(LocationRequiredFields.Region) &&
                                 string.IsNullOrEmpty(selectedLocation.Address.AdminDistrict))
                        {
                            await PromptForRequiredField(dc, LocationRequiredFields.Region, resourceManager.Region,
                                resourceManager, selectedLocation);
                        }
                        else if (requiredFields.HasFlag(LocationRequiredFields.PostalCode) &&
                                 string.IsNullOrEmpty(selectedLocation.Address.PostalCode))
                        {
                            await PromptForRequiredField(dc, LocationRequiredFields.PostalCode,
                                resourceManager.PostalCode, resourceManager, selectedLocation);
                        }
                        else if (requiredFields.HasFlag(LocationRequiredFields.Country) &&
                                 string.IsNullOrEmpty(selectedLocation.Address.CountryRegion))
                        {
                            await PromptForRequiredField(dc, LocationRequiredFields.Country, resourceManager.Country,
                                resourceManager, selectedLocation);
                        }
                        else
                        {
                            await dc.EndAsync(selectedLocation);
                        }

                        return Dialog.EndOfTurn;
                    },
                    async (dc, stepContext) =>
                    {
                        var selectedLocation = (Bing.Location) stepContext.Values[StepContextKeys.SelectedLocation];

                        switch ((LocationRequiredFields)dc.ActiveDialog.State["CurrentMissingRequiredField"])
                        {
                            case LocationRequiredFields.StreetAddress:
                                selectedLocation.Address.AddressLine = (string)stepContext.Result;
                                break;
                            case LocationRequiredFields.Locality:
                                selectedLocation.Address.Locality = (string)stepContext.Result;
                                break;
                            case LocationRequiredFields.Region:
                                selectedLocation.Address.AdminDistrict = (string)stepContext.Result;
                                break;
                            case LocationRequiredFields.PostalCode:
                                selectedLocation.Address.PostalCode = (string)stepContext.Result;
                                break;
                            case LocationRequiredFields.Country:
                                selectedLocation.Address.CountryRegion = (string)stepContext.Result;
                                break;
                        }

                        await dc.ReplaceAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                            new CompleteMissingFieldsDialogOptions { Location = selectedLocation });

                        return Dialog.EndOfTurn;
                    }
            }));

            AddDialog(new WaterfallDialog(DialogIds.HeroStartCardDialog, new WaterfallStep[]
            {
                async (dc, stepContext) =>
                {
                    await dc.Context.SendActivityAsync(CreateDialogStartHeroCard(dc.Context, resourceManager));
                    return Dialog.EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    var messageText = dc.Context.Activity.Text;

                    if (messageText.ToLower() == resourceManager.FavoriteLocations.ToLower())
                    {
                        await dc.ReplaceAsync(DialogIds.FavoriteLocationRetrieverDialog);
                    }
                    else if (messageText.ToLower() == resourceManager.OtherLocation.ToLower())
                    {
                        await dc.ReplaceAsync(DialogIds.LocationRetrieverRichDialog);
                    }
                    else
                    {
                        await dc.Context.SendActivityAsync(resourceManager.InvalidStartBranchResponse);
                        await dc.ReplaceAsync(DialogIds.HeroStartCardDialog);
                    }

                    return Dialog.EndOfTurn;
                }
        }));

            AddDialog(new WaterfallDialog(DialogIds.LocationRetrieverFacebookDialog, new WaterfallStep[]
            {
                async (dc, stepContext) =>
                {
                    if (!skipPrompt)
                    {
                        await dc.PromptAsync(PromptDialogIds.Text, new PromptOptions
                        {
                            Prompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = prompt + resourceManager.TitleSuffixFacebook
                            }
                        });
                    }

                    return Dialog.EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    var message = dc.Context.Activity.AsMessageActivity();

                    var place = message.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>())
                        .FirstOrDefault();

                    var coords = (GeoCoordinates) place?.Geo;

                    if (coords != null && coords.Latitude != null && coords.Longitude != null)
                    {
                        var location = new Bing.Location
                        {
                            Point = new GeocodePoint
                            {
                                Coordinates = new List<double>
                                {
                                    (double) coords.Latitude,
                                    (double) coords.Longitude
                                }
                            }
                        };

                        stepContext.Values[StepContextKeys.SelectedLocation] = location;
                    }
                    else
                    {
                        // If we didn't receive a valid place, post error message and restart dialog.
                        await dc.Context.SendActivityAsync(resourceManager.InvalidLocationResponseFacebook);
                        await dc.ReplaceAsync(DialogIds.LocationRetrieverFacebookDialog);
                    }

                    return Dialog.EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    var selectedLocation = (Bing.Location) stepContext.Values[StepContextKeys.SelectedLocation];
                    await dc.EndAsync(selectedLocation);
                    return Dialog.EndOfTurn;
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.ConfirmDeleteFromFavoritesDialog, new WaterfallStep[]
                    {
                        async (dc, stepContext) =>
                        {
                            var location = ((ConfirmDeleteFavoriteDialogOptions)stepContext.Options).Location;
                            stepContext.Values[StepContextKeys.SelectedLocation] = location;

                            var confirmationAsk = string.Format(
                            resourceManager.DeleteFavoriteConfirmationAsk,
                            $"{location.Name}: {location.Location.GetFormattedAddress(resourceManager.AddressSeparator)}");

                            await dc.PromptAsync(PromptDialogIds.Confirm,
                                new PromptOptions
                                {
                                    Prompt = new Activity {
                                        Type = ActivityTypes.Message,
                                        Text = confirmationAsk
                                    },
                                    RetryPrompt = new Activity {
                                        Type = ActivityTypes.Message,
                                        Text = resourceManager.ConfirmationInvalidResponse
                                    }
                                });

                            return Dialog.EndOfTurn;
                        },
                        async (dc, stepContext) =>
                        {
                            var confirmationResult = (bool)stepContext.Result;
                            var selectedLocation = (FavoriteLocation)stepContext.Values[StepContextKeys.SelectedLocation];

                            if(confirmationResult)
                            {
                                await favoritesManager.Delete(dc.Context, selectedLocation);
                                await dc.Context.SendActivityAsync(string.Format(resourceManager.FavoriteDeletedConfirmation, selectedLocation.Name));
                                await dc.ReplaceAsync(DialogIds.FavoriteLocationRetrieverDialog);
                            }
                            else
                            {
                                await dc.Context.SendActivityAsync(
                                    string.Format(resourceManager.DeleteFavoriteAbortion,
                                    selectedLocation.Name));
                                await dc.ReplaceAsync(DialogIds.FavoriteLocationRetrieverDialog);
                            }

                            return Dialog.EndOfTurn;
                        }
                    }));

            AddDialog(new WaterfallDialog(DialogIds.AddToFavoritesDialog, new WaterfallStep[]
                    {
                        async (dc, stepContext) =>
                        {
                            // If this is our first time through then get the selected location
                            // passed to us from the parent location dialog and store it in stepContext values collection
                            if (stepContext.Options != null && ((AddToFavoritesDialogOptions)stepContext.Options).Location != null)
                            {
                                stepContext.Values[StepContextKeys.SelectedLocation] = ((AddToFavoritesDialogOptions)stepContext.Options).Location;
                            }

                            // If we have a value for addToFavoritesConfirmed then we don't need to confirm again
                            // we need to skip to prompting for the name again instead
                            if (!stepContext.Values.ContainsKey("addToFavoritesConfirmed"))
                            {
                                Bing.Location selectedLocation =
                                    (Bing.Location) stepContext.Values[StepContextKeys.SelectedLocation];
                                // no capacity to add to favorites in the first place!
                                // OR the location is already marked as favorite
                                if (await favoritesManager.MaxCapacityReached(dc.Context)
                                    || await favoritesManager.IsFavorite(dc.Context, selectedLocation))
                                {
                                    await dc.EndAsync(selectedLocation);
                                }
                                else
                                {
                                    await dc.PromptAsync(PromptDialogIds.Confirm, new PromptOptions()
                                    {
                                        Prompt = new Activity
                                        {
                                            Type = ActivityTypes.Message,
                                            Text = resourceManager.AddToFavoritesAsk
                                        },
                                        RetryPrompt = new Activity
                                        {
                                            Type = ActivityTypes.Message,
                                            Text = resourceManager.AddToFavoritesRetry
                                        }
                                    });
                                }
                            }

                            return Dialog.EndOfTurn;
                        },
                        async (dc, stepContext) =>
                        {
                            bool addToFavoritesConfirmation;

                            if (!stepContext.Values.ContainsKey("addToFavoritesConfirmed"))
                            {
                                addToFavoritesConfirmation = (bool)stepContext.Result;
                            }
                            else
                            {
                                addToFavoritesConfirmation = true;
                            }

                            if (addToFavoritesConfirmation)
                            {
                                await dc.PromptAsync(PromptDialogIds.Text, new PromptOptions()
                                {
                                    Prompt = new Activity
                                    {
                                        Type = ActivityTypes.Message,
                                        Text = resourceManager.EnterNewFavoriteLocationName
                                    }
                                });
                            }
                            else
                            {
                                var selectedLocation = (Bing.Location) stepContext.Values[StepContextKeys.SelectedLocation];
                                await dc.EndAsync(selectedLocation);
                            }

                            return Dialog.EndOfTurn;
                        },
                        async (dc, stepContext) =>
                        {
                            var newFavoriteName = (string)stepContext.Result;

                            if (await favoritesManager.IsFavoriteLocationName(dc.Context, newFavoriteName))
                            {
                                await dc.Context.SendActivityAsync(string.Format(resourceManager.DuplicateFavoriteNameResponse,
                                    newFavoriteName));

                                await dc.ReplaceAsync(DialogIds.AddToFavoritesDialog,
                                    new AddToFavoritesDialogOptions()
                                    {
                                        Location = (Bing.Location) stepContext.Values[StepContextKeys.SelectedLocation]
                                    });
                            }
                            else
                            {
                                Bing.Location selectedLocation =
                                    (Bing.Location) dc.ActiveDialog.State[StepContextKeys.SelectedLocation];

                                await favoritesManager.Add(dc.Context,
                                    new FavoriteLocation {Location = selectedLocation, Name = newFavoriteName});

                                await dc.Context.SendActivityAsync(string.Format(resourceManager.FavoriteAddedConfirmation,
                                    newFavoriteName));

                                await dc.EndAsync(selectedLocation);
                            }

                            return Dialog.EndOfTurn;
                        }
                    }));

            AddDialog(new WaterfallDialog(DialogIds.FavoriteLocationRetrieverDialog, new WaterfallStep[]
            {
                async (dc, stepContext) =>
                {
                    var favorites = await favoritesManager.GetFavorites(dc.Context);

                    if (!favorites.Any())
                    {
                        await dc.Context.SendActivityAsync(resourceManager.NoFavoriteLocationsFound);
                        await dc.ReplaceAsync(DialogIds.LocationRetrieverRichDialog);
                    }
                    else
                    {
                        await dc.Context.SendActivityAsync(CreateFavoritesCarousel(dc.Context,
                            new LocationCardBuilder(apiKey, resourceManager),
                            favorites));
                        await dc.Context.SendActivityAsync(resourceManager.SelectFavoriteLocationPrompt);
                    }

                    return Dialog.EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    var messageText = dc.Context.Activity.Text;

                    if (StringComparer.OrdinalIgnoreCase.Equals(messageText, resourceManager.OtherComand))
                    {
                        await dc.ReplaceAsync(DialogIds.LocationRetrieverRichDialog);
                    }
                    else
                    {
                        var location = await TryParseSelection(dc.Context, favoritesManager, messageText);

                        if (location != null)
                        {
                            await TryReverseGeocodeAddress(location.Location, options, geoSpatialService);
                            stepContext.Values[StepContextKeys.SelectedLocation] = location;

                            if (requiredFields == LocationRequiredFields.None)
                            {
                                await dc.EndAsync(location.Location);
                            }
                            else
                            {
                                await dc.ReplaceAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                                    new CompleteMissingFieldsDialogOptions() {Location = location.Location});
                            }
                        }
                        else
                        {
                            var locationAndCommand =
                                await TryParseCommandSelection(dc.Context, favoritesManager, messageText);

                            if (locationAndCommand.Item1 != null && locationAndCommand.Item2 != null &&
                                (StringComparer.OrdinalIgnoreCase.Equals(locationAndCommand.Item2,
                                     resourceManager.DeleteCommand)
                                 || StringComparer.OrdinalIgnoreCase.Equals(locationAndCommand.Item2,
                                     resourceManager.EditCommand)))
                            {
                                if (StringComparer.OrdinalIgnoreCase.Equals(locationAndCommand.Item2,
                                    resourceManager.DeleteCommand))
                                {
                                    await dc.ReplaceAsync(DialogIds.ConfirmDeleteFromFavoritesDialog,
                                        new ConfirmDeleteFavoriteDialogOptions() {Location = locationAndCommand.Item1});
                                }
                                else
                                {
                                    await dc.Context.SendActivityAsync(
                                        "Edit Favorites functionality not currently implemented.");
                                    await dc.ReplaceAsync(DialogIds.FavoriteLocationRetrieverDialog);
                                    //var editDialog = this.locationDialogFactory.CreateDialog(BranchType.EditFavoriteLocation, value.Location, value.Name);
                                    //context.Call(editDialog, this.ResumeAfterChildDialogAsync);
                                }
                            }
                            else
                            {
                                await dc.Context.SendActivityAsync(
                                    string.Format(resourceManager.InvalidFavoriteLocationSelection, messageText));
                                await dc.ReplaceAsync(DialogIds.FavoriteLocationRetrieverDialog);
                            }
                        }
                    }

                    return Dialog.EndOfTurn;
                }
            }));
        }

        private async Task<FavoriteLocation> TryParseSelection(ITurnContext context, IFavoritesManager favoritesManager, string text)
        {
            var favoriteRetrievedByName = await favoritesManager.GetFavoriteByName(context, text);

            if (favoriteRetrievedByName != null)
            {
                return favoriteRetrievedByName;
            }

            int index = -1;

            if (int.TryParse(text, out index))
            {
                return await favoritesManager.GetFavoriteByIndex(context, index - 1);
            }

            return null;
        }

        private async Task<(FavoriteLocation, string)> TryParseCommandSelection(ITurnContext context, IFavoritesManager favoritesManager, string text)
        {
            FavoriteLocation value = null;
            string command = null;

            var tokens = text.Split(' ');
            if (tokens.Length != 2)
                return (null, null);

            command = tokens[0];

            value = await TryParseSelection(context, favoritesManager, tokens[1]);

            return (value, command);
        }

        private static async Task PromptForRequiredField(DialogContext dc, LocationRequiredFields requiredField, string requiredFieldPrompt, LocationResourceManager resourceManager,
            Bing.Location selectedLocation)
        {
            var formattedAddress = selectedLocation.GetFormattedAddress(resourceManager.AddressSeparator);

            dc.ActiveDialog.State["CurrentMissingRequiredField"] = requiredField;
            await dc.PromptAsync(PromptDialogIds.Text,
                new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Text = string.Format(resourceManager.AskForPrefix, formattedAddress) +
                                         string.Format(resourceManager.AskForTemplate,
                                             requiredFieldPrompt)
                    }
                });
        }

        private IMessageActivity CreateFavoritesCarousel(ITurnContext context, ILocationCardBuilder cardBuilder, List<FavoriteLocation> locations)
        {
            // Get cards for the favorite locations
            var attachments = cardBuilder.CreateHeroCards(locations.Select(f => f.Location).ToList(), alwaysShowNumericPrefix: true, locationNames: locations.Select(f => f.Name).ToList());
            var message = context.Activity.CreateReply();
            message.Attachments = attachments.Select(c => c.ToAttachment()).ToList();
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return message;
        }

        private static Activity CreateDialogStartHeroCard(ITurnContext context, LocationResourceManager resourceManager)
        {
            var dialogStartCard = context.Activity.CreateReply();
            var buttons = new List<CardAction>();

            var branches = new string[] { resourceManager.FavoriteLocations, resourceManager.OtherLocation };

            foreach (var possibleBranch in branches)
            {
                buttons.Add(new CardAction
                {
                    Type = "imBack",
                    Title = possibleBranch,
                    Value = possibleBranch
                });
            }

            var heroCard = new HeroCard
            {
                Subtitle = resourceManager.DialogStartBranchAsk,
                Buttons = buttons
            };

            dialogStartCard.Attachments = new List<Attachment> { heroCard.ToAttachment() };
            dialogStartCard.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            return dialogStartCard;
        }

        private static Place CreatePlace(Bing.Location location)
        {
            var place = new Place
            {
                Type = location.EntityType,
                Name = location.Name
            };

            if (location.Address != null)
            {
                place.Address = new PostalAddress
                {
                    FormattedAddress = location.Address.FormattedAddress,
                    Country = location.Address.CountryRegion,
                    Locality = location.Address.Locality,
                    PostalCode = location.Address.PostalCode,
                    Region = location.Address.AdminDistrict,
                    StreetAddress = location.Address.AddressLine
                };
            }

            if (location.Point != null && location.Point.HasCoordinates)
            {
                place.Geo = new GeoCoordinates
                {
                    Latitude = location.Point.Coordinates[0],
                    Longitude = location.Point.Coordinates[1]
                };
            }

            return place;
        }

        private static async Task TryReverseGeocodeAddress(Bing.Location location, LocationOptions options, IGeoSpatialService geoSpatialService)
        {
            // If user passed ReverseGeocode flag and dialog returned a geo point,
            // then try to reverse geocode it using BingGeoSpatialService.
            if (options.HasFlag(LocationOptions.ReverseGeocode) && location != null && location.Address == null && location.Point != null)
            {
                var results = await geoSpatialService.GetLocationsByPointAsync(location.Point.Coordinates[0], location.Point.Coordinates[1]);
                var geocodedLocation = results?.Locations?.FirstOrDefault();
                if (geocodedLocation?.Address != null)
                {
                    // We don't trust reverse geo-coder on the street address level,
                    // so copy all fields except it.
                    // TODO: do we need to check the returned confidence level?
                    location.Address = new Address
                    {
                        CountryRegion = geocodedLocation.Address.CountryRegion,
                        AdminDistrict = geocodedLocation.Address.AdminDistrict,
                        AdminDistrict2 = geocodedLocation.Address.AdminDistrict2,
                        Locality = geocodedLocation.Address.Locality,
                        PostalCode = geocodedLocation.Address.PostalCode
                    };
                }
            }
        }
    }
}

