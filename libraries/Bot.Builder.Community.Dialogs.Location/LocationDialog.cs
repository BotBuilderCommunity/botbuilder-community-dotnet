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
        
        public const string LocationDialogId = "LocationDialog";

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
            string dialogId = LocationDialogId,
            string apiKey,
            string prompt,
            BotState state,
            bool skipPrompt = false,
            bool useAzureMaps = true,
            LocationOptions options = LocationOptions.None,
            LocationRequiredFields requiredFields = LocationRequiredFields.None,
            LocationResourceManager resourceManager = null) : base(dialogId)
        {
            resourceManager = resourceManager ?? new LocationResourceManager();

            if (!options.HasFlag(LocationOptions.SkipFavorites) && state == null)
            {
                throw new ArgumentNullException(nameof(state),
                    "If LocationOptions.SkipFavorites is not used then BotState object must be " +
                    "provided to allow for storing / retrieval of favorites");
            }

            var favoriteLocations = state.CreateProperty<List<FavoriteLocation>>($"{nameof(LocationDialog)}.Favorites");
            var favoritesManager = new FavoritesManager(favoriteLocations);

            IGeoSpatialService geoSpatialService;
            if (useAzureMaps)
            {
                geoSpatialService = new AzureMapsSpatialService(apiKey);
            }
            else
            {
                geoSpatialService = new BingGeoSpatialService(apiKey);
            }

            InitialDialogId = LocationDialogId;

            AddDialog(new ChoicePrompt(PromptDialogIds.Choice));
            AddDialog(new TextPrompt(PromptDialogIds.Text));
            AddDialog(new ConfirmPrompt(PromptDialogIds.Confirm));
            
            AddDialog(new WaterfallDialog(InitialDialogId, new WaterfallStep[]
            {
                async (dc, cancellationToken) =>
                {
                    if (options.HasFlag(LocationOptions.SkipFavorites)
                        || !favoritesManager.GetFavorites(dc.Context).Result.Any())
                    {
                        var isFacebookChannel = StringComparer.OrdinalIgnoreCase.Equals(
                            dc.Context.Activity.ChannelId, "facebook");

                        if (options.HasFlag(LocationOptions.UseNativeControl) && isFacebookChannel)
                        {
                            return await dc.BeginDialogAsync(DialogIds.LocationRetrieverFacebookDialog);
                        }

                        return await dc.BeginDialogAsync(DialogIds.LocationRetrieverRichDialog);
                    }

                    return await dc.BeginDialogAsync(DialogIds.HeroStartCardDialog);
                },
                async (dc, cancellationToken) =>
                {
                    var selectedLocation = (Bing.Location) dc.Result;
                    dc.Values[StepContextKeys.SelectedLocation] = selectedLocation;

                    if (options.HasFlag(LocationOptions.SkipFinalConfirmation))
                    {
                        return await dc.NextAsync();
                    }

                    await dc.PromptAsync(PromptDialogIds.Confirm,
                        new PromptOptions()
                        {
                            Prompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = string.Format(resourceManager.ConfirmationAsk,
                                    selectedLocation.GetFormattedAddress(resourceManager.AddressSeparator))
                            },
                            RetryPrompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = resourceManager.ConfirmationInvalidResponse
                            }
                        });

                    return EndOfTurn;
                },
                async (dc, cancellationToken) =>
                {
                    if (dc.Result is bool result && !result)
                    {
                        await dc.Context.SendActivityAsync(resourceManager.ResetPrompt);
                        return await dc.ReplaceDialogAsync(InitialDialogId);
                    }

                    if (!options.HasFlag(LocationOptions.SkipFavorites))
                    {
                        return await dc.BeginDialogAsync(DialogIds.AddToFavoritesDialog,
                            new AddToFavoritesDialogOptions()
                            {
                                Location = (Bing.Location) dc.Values[StepContextKeys.SelectedLocation]
                            }
                        );
                    }

                    return EndOfTurn;
                },
                async (dc, cancellationToken) =>
                {
                    var selectedLocation = (Bing.Location) dc.Values[StepContextKeys.SelectedLocation];
                    return await dc.EndDialogAsync(CreatePlace(selectedLocation));
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.LocationRetrieverRichDialog, new WaterfallStep[]
            {
                async (dc, cancellationToken) =>
                {
                    if (skipPrompt)
                    {
                        return await dc.NextAsync();
                    }

                    return await dc.PromptAsync(PromptDialogIds.Text, new PromptOptions()
                    {
                        Prompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = prompt + resourceManager.TitleSuffix
                        }
                    });
                },
                async (dc, cancellationToken) =>
                {
                    var locationQuery = ((string) dc.Result);
                    var locationSet = await geoSpatialService.GetLocationsByQueryAsync(locationQuery);
                    var foundLocations = locationSet?.Locations;

                    if (foundLocations == null || foundLocations.Count == 0)
                    {
                        await dc.Context.SendActivityAsync(resourceManager.LocationNotFound);
                        return await dc.ReplaceDialogAsync(DialogIds.LocationRetrieverRichDialog);
                    }

                    var locations = new List<Bing.Location>();
                    locations.AddRange(foundLocations.Take(MaxLocationCount));

                    return await dc.BeginDialogAsync(DialogIds.SelectAndConfirmLocationDialog,
                        new SelectLocationDialogOptions {Locations = locations});
                },
                async (dc, cancellationToken) =>
                {
                    var selectedLocation = (Bing.Location) dc.Result;
                    return await dc.EndDialogAsync(selectedLocation);
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.SelectAndConfirmLocationDialog, new WaterfallStep[]
            {
                async (dc, cancellationToken) =>
                {
                    var locations = ((SelectLocationDialogOptions) dc.Options).Locations;
                    dc.Values[StepContextKeys.Locations] = locations;

                    var locationsCardReply = dc.Context.Activity.CreateReply();
                    var cardBuilder = new LocationCardBuilder(apiKey, resourceManager);

                    locationsCardReply.Attachments = cardBuilder.CreateHeroCards(locations)
                        .Select(c => c.ToAttachment()).ToList();

                    locationsCardReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    await dc.Context.SendActivityAsync(locationsCardReply);

                    if (locations.Count == 1)
                    {
                        return await dc.PromptAsync(PromptDialogIds.Confirm,
                            new PromptOptions()
                            {
                                Prompt = new Activity
                                {
                                    Type = ActivityTypes.Message,
                                    Text = resourceManager.SingleResultFound
                                },
                                RetryPrompt = new Activity
                                {
                                    Type = ActivityTypes.Message,
                                    Text = resourceManager.ConfirmationInvalidResponse
                                }
                            });
                    }

                    await dc.Context.SendActivityAsync(resourceManager.MultipleResultsFound);
                    return EndOfTurn;
                },
                async (dc, cancellationToken) =>
                {
                    var locations = (List<Bing.Location>) dc.Values[StepContextKeys.Locations];

                    if (dc.Result is bool result)
                    {
                        if (result)
                        {
                            await TryReverseGeocodeAddress(locations.First(), options, geoSpatialService);

                            if (requiredFields == LocationRequiredFields.None)
                            {
                                return await dc.EndDialogAsync(locations.First());
                            }

                            return await dc.ReplaceDialogAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                                new CompleteMissingFieldsDialogOptions {Location = locations.First()});
                        }

                        return await dc.ReplaceDialogAsync(DialogIds.LocationRetrieverRichDialog);
                    }

                    var message = dc.Context.Activity.Text;

                    if (int.TryParse(message, out var value) && value > 0 && value <= locations.Count)
                    {
                        await TryReverseGeocodeAddress(locations[value - 1], options, geoSpatialService);

                        if (requiredFields == LocationRequiredFields.None)
                        {
                            return await dc.EndDialogAsync(locations[value - 1]);
                        }

                        return await dc.ReplaceDialogAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                            new CompleteMissingFieldsDialogOptions {Location = locations[value - 1]});
                    }

                    if (StringComparer.OrdinalIgnoreCase.Equals(message, resourceManager.OtherComand))
                    {
                        return await dc.ReplaceDialogAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                            new CompleteMissingFieldsDialogOptions {Location = new Bing.Location()});
                    }

                    await dc.Context.SendActivityAsync(resourceManager.InvalidLocationResponse);

                    return await dc.ReplaceDialogAsync(DialogIds.SelectAndConfirmLocationDialog,
                        new SelectLocationDialogOptions
                        {
                            Locations = (List<Bing.Location>) dc.Values[StepContextKeys.Locations]
                        });
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.CompleteMissingRequiredFieldsDialog, new WaterfallStep[]
            {
                async (dc, cancellationToken) =>
                {
                    var selectedLocation = ((CompleteMissingFieldsDialogOptions) dc.Options).Location;

                    dc.Values[StepContextKeys.SelectedLocation] = selectedLocation;

                    if (requiredFields.HasFlag(LocationRequiredFields.StreetAddress) &&
                        string.IsNullOrEmpty(selectedLocation.Address.AddressLine))
                    {
                        return await PromptForRequiredField(dc, LocationRequiredFields.StreetAddress,
                            resourceManager.StreetAddress, resourceManager, selectedLocation);
                    }

                    if (requiredFields.HasFlag(LocationRequiredFields.Locality) &&
                        string.IsNullOrEmpty(selectedLocation.Address.Locality))
                    {
                        return await PromptForRequiredField(dc, LocationRequiredFields.Locality,
                            resourceManager.Locality,
                            resourceManager, selectedLocation);
                    }

                    if (requiredFields.HasFlag(LocationRequiredFields.Region) &&
                        string.IsNullOrEmpty(selectedLocation.Address.AdminDistrict))
                    {
                        return await PromptForRequiredField(dc, LocationRequiredFields.Region, resourceManager.Region,
                            resourceManager, selectedLocation);
                    }

                    if (requiredFields.HasFlag(LocationRequiredFields.PostalCode) &&
                        string.IsNullOrEmpty(selectedLocation.Address.PostalCode))
                    {
                        return await PromptForRequiredField(dc, LocationRequiredFields.PostalCode,
                            resourceManager.PostalCode, resourceManager, selectedLocation);
                    }

                    if (requiredFields.HasFlag(LocationRequiredFields.Country) &&
                        string.IsNullOrEmpty(selectedLocation.Address.CountryRegion))
                    {
                        return await PromptForRequiredField(dc, LocationRequiredFields.Country, resourceManager.Country,
                            resourceManager, selectedLocation);
                    }

                    return await dc.EndDialogAsync(selectedLocation);
                },
                async (dc, cancellationToken) =>
                {
                    var selectedLocation = (Bing.Location) dc.Values[StepContextKeys.SelectedLocation];

                    switch ((LocationRequiredFields) dc.ActiveDialog.State["CurrentMissingRequiredField"])
                    {
                        case LocationRequiredFields.StreetAddress:
                            selectedLocation.Address.AddressLine = (string) dc.Result;
                            break;
                        case LocationRequiredFields.Locality:
                            selectedLocation.Address.Locality = (string) dc.Result;
                            break;
                        case LocationRequiredFields.Region:
                            selectedLocation.Address.AdminDistrict = (string) dc.Result;
                            break;
                        case LocationRequiredFields.PostalCode:
                            selectedLocation.Address.PostalCode = (string) dc.Result;
                            break;
                        case LocationRequiredFields.Country:
                            selectedLocation.Address.CountryRegion = (string) dc.Result;
                            break;
                    }

                    return await dc.ReplaceDialogAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                        new CompleteMissingFieldsDialogOptions {Location = selectedLocation});
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.HeroStartCardDialog, new WaterfallStep[]
            {
                async (dc, cancellationToken) =>
                {
                    await dc.Context.SendActivityAsync(CreateDialogStartHeroCard(dc.Context, resourceManager));
                    return EndOfTurn;
                },
                async (dc, cancellationToken) =>
                {
                    var messageText = dc.Context.Activity.Text;

                    if (messageText.ToLower() == resourceManager.FavoriteLocations.ToLower())
                    {
                        return await dc.ReplaceDialogAsync(DialogIds.FavoriteLocationRetrieverDialog);
                    }

                    if (messageText.ToLower() == resourceManager.OtherLocation.ToLower())
                    {
                        return await dc.ReplaceDialogAsync(DialogIds.LocationRetrieverRichDialog);
                    }

                    await dc.Context.SendActivityAsync(resourceManager.InvalidStartBranchResponse);
                    return await dc.ReplaceDialogAsync(DialogIds.HeroStartCardDialog);
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.LocationRetrieverFacebookDialog, new WaterfallStep[]
            {
                async (dc, cancellationToken) =>
                {
                    if (skipPrompt)
                    {
                        return await dc.NextAsync();
                    }

                    return await dc.PromptAsync(PromptDialogIds.Text, new PromptOptions
                    {
                        Prompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = prompt + resourceManager.TitleSuffixFacebook
                        }
                    });
                },
                async (dc, cancellationToken) =>
                {
                    var message = dc.Context.Activity.AsMessageActivity();

                    var place = message.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>())
                        .FirstOrDefault();

                    var coords = (GeoCoordinates) place?.Geo;

                    if (coords?.Latitude != null && coords.Longitude != null)
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

                        dc.Values[StepContextKeys.SelectedLocation] = location;

                        return EndOfTurn;
                    }

                    // If we didn't receive a valid place, post error message and restart dialog.
                    await dc.Context.SendActivityAsync(resourceManager.InvalidLocationResponseFacebook);
                    return await dc.ReplaceDialogAsync(DialogIds.LocationRetrieverFacebookDialog);
                },
                async (dc, cancellationToken) =>
                {
                    var selectedLocation = (Bing.Location) dc.Values[StepContextKeys.SelectedLocation];
                    return await dc.EndDialogAsync(selectedLocation);
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.ConfirmDeleteFromFavoritesDialog, new WaterfallStep[]
                    {
                        async (dc, cancellationToken) =>
                        {
                            var location = ((ConfirmDeleteFavoriteDialogOptions)dc.Options).Location;
                            dc.Values[StepContextKeys.SelectedLocation] = location;

                            var confirmationAsk = string.Format(
                                resourceManager.DeleteFavoriteConfirmationAsk,
                                $"{location.Name}: {location.Location.GetFormattedAddress(resourceManager.AddressSeparator)}");

                            return await dc.PromptAsync(PromptDialogIds.Confirm,
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
                        },
                        async (dc, cancellationToken) =>
                        {
                            var confirmationResult = (bool) dc.Result;
                            var selectedLocation =
                                (FavoriteLocation) dc.Values[StepContextKeys.SelectedLocation];

                            if (confirmationResult)
                            {
                                await favoritesManager.Delete(dc.Context, selectedLocation);
                                await dc.Context.SendActivityAsync(
                                    string.Format(resourceManager.FavoriteDeletedConfirmation, selectedLocation.Name));
                                return await dc.ReplaceDialogAsync(DialogIds.FavoriteLocationRetrieverDialog);
                            }

                            await dc.Context.SendActivityAsync(
                                string.Format(resourceManager.DeleteFavoriteAbortion,
                                    selectedLocation.Name));
                            return await dc.ReplaceDialogAsync(DialogIds.FavoriteLocationRetrieverDialog);
                        }
                    }));

            AddDialog(new WaterfallDialog(DialogIds.AddToFavoritesDialog, new WaterfallStep[]
            {
                async (dc, cancellationToken) =>
                {
                    // If this is our first time through then get the selected location
                    // passed to us from the parent location dialog and store it in stepContext values collection
                    if (((AddToFavoritesDialogOptions) dc.Options)?.Location != null)
                    {
                        dc.Values[StepContextKeys.SelectedLocation] =
                            ((AddToFavoritesDialogOptions) dc.Options).Location;
                    }

                    // If we have a value for addToFavoritesConfirmed then we don't need to confirm again
                    // we need to skip to prompting for the name again instead
                    if (!dc.Values.ContainsKey("addToFavoritesConfirmed"))
                    {
                        var selectedLocation =
                            (Bing.Location) dc.Values[StepContextKeys.SelectedLocation];
                        // no capacity to add to favorites in the first place!
                        // OR the location is already marked as favorite
                        if (await favoritesManager.MaxCapacityReached(dc.Context)
                            || await favoritesManager.IsFavorite(dc.Context, selectedLocation))
                        {
                            return await dc.EndDialogAsync(selectedLocation);
                        }

                        return await dc.PromptAsync(PromptDialogIds.Confirm, new PromptOptions()
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

                    return await dc.NextAsync();
                },
                async (dc, cancellationToken) =>
                {
                    bool addToFavoritesConfirmation;

                    if (!dc.Values.ContainsKey("addToFavoritesConfirmed"))
                    {
                        addToFavoritesConfirmation = (bool) dc.Result;
                    }
                    else
                    {
                        addToFavoritesConfirmation = true;
                    }

                    if (addToFavoritesConfirmation)
                    {
                        return await dc.PromptAsync(PromptDialogIds.Text, new PromptOptions()
                        {
                            Prompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = resourceManager.EnterNewFavoriteLocationName
                            }
                        });
                    }

                    var selectedLocation = (Bing.Location) dc.Values[StepContextKeys.SelectedLocation];
                    return await dc.EndDialogAsync(selectedLocation);
                },
                async (dc, cancellationToken) =>
                {
                    var newFavoriteName = (string) dc.Result;

                    if (await favoritesManager.IsFavoriteLocationName(dc.Context, newFavoriteName))
                    {
                        await dc.Context.SendActivityAsync(string.Format(
                            resourceManager.DuplicateFavoriteNameResponse,
                            newFavoriteName));

                        return await dc.ReplaceDialogAsync(DialogIds.AddToFavoritesDialog,
                            new AddToFavoritesDialogOptions()
                            {
                                Location = (Bing.Location) dc.Values[StepContextKeys.SelectedLocation]
                            });
                    }

                    var selectedLocation =
                        (Bing.Location) dc.Values[StepContextKeys.SelectedLocation];

                    await favoritesManager.Add(dc.Context,
                        new FavoriteLocation {Location = selectedLocation, Name = newFavoriteName});

                    await dc.Context.SendActivityAsync(string.Format(resourceManager.FavoriteAddedConfirmation,
                        newFavoriteName));

                    return await dc.EndDialogAsync(selectedLocation);
                }
            }));

            AddDialog(new WaterfallDialog(DialogIds.FavoriteLocationRetrieverDialog, new WaterfallStep[]
            {
                async (dc, cancellationToken) =>
                {
                    var favorites = await favoritesManager.GetFavorites(dc.Context);

                    if (!favorites.Any())
                    {
                        await dc.Context.SendActivityAsync(resourceManager.NoFavoriteLocationsFound);
                        return await dc.ReplaceDialogAsync(DialogIds.LocationRetrieverRichDialog);
                    }

                    await dc.Context.SendActivityAsync(CreateFavoritesCarousel(dc.Context,
                        new LocationCardBuilder(apiKey, resourceManager),
                        favorites));
                    await dc.Context.SendActivityAsync(resourceManager.SelectFavoriteLocationPrompt);
                    return EndOfTurn;
                },
                async (dc, cancellationToken) =>
                {
                    var messageText = dc.Context.Activity.Text;

                    if (StringComparer.OrdinalIgnoreCase.Equals(messageText, resourceManager.OtherComand))
                    {
                        return await dc.ReplaceDialogAsync(DialogIds.LocationRetrieverRichDialog);
                    }

                    var location = await TryParseSelection(dc.Context, favoritesManager, messageText);

                    if (location != null)
                    {
                        await TryReverseGeocodeAddress(location.Location, options, geoSpatialService);
                        dc.Values[StepContextKeys.SelectedLocation] = location;

                        if (requiredFields == LocationRequiredFields.None)
                        {
                            return await dc.EndDialogAsync(location.Location);
                        }

                        return await dc.ReplaceDialogAsync(DialogIds.CompleteMissingRequiredFieldsDialog,
                            new CompleteMissingFieldsDialogOptions() {Location = location.Location});
                    }

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
                            return await dc.ReplaceDialogAsync(DialogIds.ConfirmDeleteFromFavoritesDialog,
                                new ConfirmDeleteFavoriteDialogOptions() {Location = locationAndCommand.Item1});
                        }

                        await dc.Context.SendActivityAsync(
                            "Edit Favorites functionality not currently implemented.");
                        return await dc.ReplaceDialogAsync(DialogIds.FavoriteLocationRetrieverDialog);
                        //var editDialog = this.locationDialogFactory.CreateDialog(BranchType.EditFavoriteLocation, value.Location, value.Name);
                        //context.Call(editDialog, this.ResumeAfterChildDialogAsync);
                    }

                    await dc.Context.SendActivityAsync(
                        string.Format(resourceManager.InvalidFavoriteLocationSelection, messageText));

                    return await dc.ReplaceDialogAsync(DialogIds.FavoriteLocationRetrieverDialog);
                }
            }));
        }

        private static async Task<FavoriteLocation> TryParseSelection(ITurnContext context, IFavoritesManager favoritesManager, string text)
        {
            var favoriteRetrievedByName = await favoritesManager.GetFavoriteByName(context, text);

            if (favoriteRetrievedByName != null)
            {
                return favoriteRetrievedByName;
            }

            if (int.TryParse(text, out var index))
            {
                return await favoritesManager.GetFavoriteByIndex(context, index - 1);
            }

            return null;
        }

        private static async Task<(FavoriteLocation, string)> TryParseCommandSelection(ITurnContext context, IFavoritesManager favoritesManager, string text)
        {
            var tokens = text.Split(' ');

            if (tokens.Length != 2)
                return (null, null);

            var command = tokens[0];

            var value = await TryParseSelection(context, favoritesManager, tokens[1]);

            return (value, command);
        }

        private static async Task<DialogTurnResult> PromptForRequiredField(DialogContext dc, LocationRequiredFields requiredField, string requiredFieldPrompt, LocationResourceManager resourceManager,
            Bing.Location selectedLocation)
        {
            var formattedAddress = selectedLocation.GetFormattedAddress(resourceManager.AddressSeparator);

            dc.ActiveDialog.State["CurrentMissingRequiredField"] = requiredField;

            return await dc.PromptAsync(PromptDialogIds.Text,
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

        private static IMessageActivity CreateFavoritesCarousel(ITurnContext context, ILocationCardBuilder cardBuilder, List<FavoriteLocation> locations)
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

