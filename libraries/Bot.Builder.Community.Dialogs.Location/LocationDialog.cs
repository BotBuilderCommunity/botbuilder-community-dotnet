using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.Location.Azure;
using Bot.Builder.Community.Dialogs.Location.Bing;
using Bot.Builder.Community.Dialogs.Location.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
using ConfirmPrompt = Microsoft.Bot.Builder.Dialogs.ConfirmPrompt;
using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

namespace Bot.Builder.Community.Dialogs.Location
{
    public class LocationDialog : DialogContainer
    {
        public const string MainDialogId = "LocationDialog";

        private const int MaxLocationCount = 5;

        /// <summary>Contains the IDs for the other dialogs in the set.</summary>
        private static class DialogIds
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
        private static class Inputs
        {
            public const string Text = "textPrompt";
            public const string Choice = "choicePrompt";
            public const string Confirm = "confirmPrompt";
        }

        private static class Outputs
        {
            public const string Locations = "locations";
            public const string SelectedLocation = "selectedLocation";
        }

        public LocationDialog(
            string apiKey,
            string prompt,
            bool skipPrompt = false,
            bool useAzureMaps = true,
            LocationOptions options = LocationOptions.None,
            LocationRequiredFields requiredFields = LocationRequiredFields.None,
            LocationResourceManager resourceManager = null) : base(MainDialogId)
        {
            resourceManager = resourceManager ?? new LocationResourceManager();
            var favoritesManager = new FavoritesManager();

            IGeoSpatialService geoSpatialService;
            if (useAzureMaps)
            {
                geoSpatialService = new AzureMapsSpatialService(apiKey);
            }
            else
            {
                geoSpatialService = new BingGeoSpatialService(apiKey);
            }

            Dialogs.Add(Inputs.Choice, new ChoicePrompt(Culture.English));
            Dialogs.Add(Inputs.Text, new TextPrompt());
            Dialogs.Add(Inputs.Confirm, new ConfirmPrompt(Culture.English));

            Dialogs.Add(MainDialogId, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    if (options.HasFlag(LocationOptions.SkipFavorites)
                        || !favoritesManager.GetFavorites(dc.Context).Result.Any())
                    {
                        var isFacebookChannel = StringComparer.OrdinalIgnoreCase.Equals(
                            dc.Context.Activity.ChannelId, "facebook");

                        if (options.HasFlag(LocationOptions.UseNativeControl) && isFacebookChannel)
                        {
                            await dc.Begin(DialogIds.LocationRetrieverFacebookDialog);
                        }
                        else
                        {
                            await dc.Begin(DialogIds.LocationRetrieverRichDialog);
                        }
                    }
                    else
                    {
                        await dc.Begin(DialogIds.HeroStartCardDialog);
                    }
                },
                async (dc, args, next) =>
                {
                    Bing.Location selectedLocation = (Bing.Location) args[Outputs.SelectedLocation];
                    dc.ActiveDialog.State[Outputs.SelectedLocation] = selectedLocation;

                    if (options.HasFlag(LocationOptions.SkipFinalConfirmation))
                    {
                        await next();
                    }
                    else
                    {
                        await dc.Prompt(Inputs.Confirm,
                            string.Format(resourceManager.ConfirmationAsk,
                                selectedLocation.GetFormattedAddress(resourceManager.AddressSeparator)),
                            new PromptOptions()
                            {
                                RetryPromptString = resourceManager.ConfirmationInvalidResponse
                            });
                    }
                },
                async (dc, args, next) =>
                {
                    if (args is ConfirmResult result && !result.Confirmation)
                    {
                        await dc.Context.SendActivity(resourceManager.ResetPrompt);
                        await dc.Replace(MainDialogId);
                    }
                    else
                    {
                        if (!options.HasFlag(LocationOptions.SkipFavorites))
                        {
                            await dc.Begin(DialogIds.AddToFavoritesDialog, dc.ActiveDialog.State);
                        }
                    }
                },
                async (dc, args, next) =>
                {
                    Bing.Location selectedLocation = (Bing.Location) dc.ActiveDialog.State[Outputs.SelectedLocation];
                    await dc.End(new LocationDialogResult()
                    {
                        SelectedLocation = CreatePlace(selectedLocation)
                    });
                }
            });

            Dialogs.Add(DialogIds.HeroStartCardDialog, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await dc.Context.SendActivity(CreateDialogStartHeroCard(dc.Context, resourceManager));
                },
                async (dc, args, next) =>
                {
                    var messageText = dc.Context.Activity.Text;

                    if (messageText.ToLower() == resourceManager.FavoriteLocations.ToLower())
                    {
                        await dc.Replace(DialogIds.FavoriteLocationRetrieverDialog);
                    }
                    else if (messageText.ToLower() == resourceManager.OtherLocation.ToLower())
                    {
                        await dc.Replace(DialogIds.LocationRetrieverRichDialog);
                    }
                    else
                    {
                        await dc.Context.SendActivity(resourceManager.InvalidStartBranchResponse);
                        await dc.Replace(DialogIds.HeroStartCardDialog);
                    }
                }
            });

            Dialogs.Add(DialogIds.FavoriteLocationRetrieverDialog, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    var favorites = await favoritesManager.GetFavorites(dc.Context);

                    if (!favorites.Any())
                    {
                        await dc.Context.SendActivity(resourceManager.NoFavoriteLocationsFound);
                        await dc.Replace(DialogIds.LocationRetrieverRichDialog);
                    }
                    else
                    {
                        await dc.Context.SendActivity(CreateFavoritesCarousel(dc.Context,
                            new LocationCardBuilder(apiKey, resourceManager),
                            favorites));
                        await dc.Context.SendActivity(resourceManager.SelectFavoriteLocationPrompt);
                    }
                },
                async (dc, args, next) =>
                {
                    var messageText = dc.Context.Activity.Text;
                    string command = null;

                    if (StringComparer.OrdinalIgnoreCase.Equals(messageText, resourceManager.OtherComand))
                    {
                        await dc.Replace(DialogIds.LocationRetrieverRichDialog);
                    }
                    else
                    {
                        var location = await TryParseSelection(dc.Context, favoritesManager, messageText);

                        if(location != null)
                        {
                            await TryReverseGeocodeAddress(location.Location, options, geoSpatialService);
                            dc.ActiveDialog.State[Outputs.SelectedLocation] = location;

                            if (requiredFields == LocationRequiredFields.None)
                            {
                                await dc.End(new Dictionary<string, object>
                                {
                                    {Outputs.SelectedLocation, location.Location}
                                });
                            }
                            else
                            {
                                await dc.Replace(DialogIds.CompleteMissingRequiredFieldsDialog, new Dictionary<string, object>
                                {
                                    {Outputs.SelectedLocation, location.Location}
                                });
                            }
                        }
                        else
                        {
                            var locationAndCommand = await TryParseCommandSelection(dc.Context, favoritesManager, messageText);

                            if(locationAndCommand.Item1 != null && locationAndCommand.Item2 != null &&
                            (StringComparer.OrdinalIgnoreCase.Equals(locationAndCommand.Item2, resourceManager.DeleteCommand)
                            || StringComparer.OrdinalIgnoreCase.Equals(locationAndCommand.Item2, resourceManager.EditCommand)))
                            {
                                if (StringComparer.OrdinalIgnoreCase.Equals(locationAndCommand.Item2, resourceManager.DeleteCommand))
                                {
                                    await dc.Replace(DialogIds.ConfirmDeleteFromFavoritesDialog,
                                        new Dictionary<string, object>
                                        {
                                            {Outputs.SelectedLocation, locationAndCommand.Item1}
                                        });
                                }
                                else
                                {
                                    await dc.Context.SendActivity("Edit Favorites functionality not currently implemented.");
                                    await dc.Replace(DialogIds.FavoriteLocationRetrieverDialog);
                                    //var editDialog = this.locationDialogFactory.CreateDialog(BranchType.EditFavoriteLocation, value.Location, value.Name);
                                    //context.Call(editDialog, this.ResumeAfterChildDialogAsync);
                                }
                            }
                            else
                            {
                                await dc.Context.SendActivity(string.Format(resourceManager.InvalidFavoriteLocationSelection, messageText));
                                await dc.Replace(DialogIds.FavoriteLocationRetrieverDialog);
                            }
                        }
                    }
                }
            });

            Dialogs.Add(DialogIds.LocationRetrieverRichDialog, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    if (!skipPrompt)
                    {
                        await dc.Prompt(Inputs.Text, prompt + resourceManager.TitleSuffix);
                    }
                },
                async (dc, args, next) =>
                {
                    var locationQuery = ((TextResult) args).Text;
                    var locationSet = await geoSpatialService.GetLocationsByQueryAsync(locationQuery);
                    var foundLocations = locationSet?.Locations;

                    if (foundLocations == null || foundLocations.Count == 0)
                    {
                        await dc.Context.SendActivity(resourceManager.LocationNotFound);
                        await dc.Replace(DialogIds.LocationRetrieverRichDialog);
                    }
                    else
                    {
                        var locations = new List<Bing.Location>();
                        locations.AddRange(foundLocations.Take(MaxLocationCount));

                        await dc.Begin(DialogIds.SelectAndConfirmLocationDialog,
                            new Dictionary<string, object> {{Outputs.Locations, locations}});
                    }
                },
                async (dc, args, next) =>
                {
                    var selectedLocation = (Bing.Location) args[Outputs.SelectedLocation];
                    await dc.End(new Dictionary<string, object>
                    {
                        {Outputs.SelectedLocation, selectedLocation}
                    });
                }
            });

            Dialogs.Add(DialogIds.LocationRetrieverFacebookDialog, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    if (!skipPrompt)
                    {
                        await dc.Prompt(Inputs.Text, prompt + resourceManager.TitleSuffixFacebook);
                    }
                },
                async (dc, args, next) =>
                {
                    var message = dc.Context.Activity.AsMessageActivity();

                    var place = message.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();

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

                        dc.ActiveDialog.State[Outputs.SelectedLocation] = location;
                    }
                    else
                    {
                        // If we didn't receive a valid place, post error message and restart dialog.
                        await dc.Context.SendActivity(resourceManager.InvalidLocationResponseFacebook);
                        await dc.Replace(DialogIds.LocationRetrieverFacebookDialog);
                    }
                },
                async (dc, args, next) =>
                {
                    var selectedLocation = (Bing.Location) args[Outputs.SelectedLocation];
                    await dc.End(new Dictionary<string, object>
                    {
                        {Outputs.SelectedLocation, selectedLocation}
                    });
                }
            });

            Dialogs.Add(DialogIds.SelectAndConfirmLocationDialog, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    var locations = (List<Bing.Location>) args[Outputs.Locations];
                    dc.ActiveDialog.State[Outputs.Locations] = locations;

                    var locationsCardReply = dc.Context.Activity.CreateReply();
                    var cardBuilder = new LocationCardBuilder(apiKey, resourceManager);
                    locationsCardReply.Attachments = cardBuilder.CreateHeroCards(locations)
                        .Select(C => C.ToAttachment()).ToList();
                    locationsCardReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    await dc.Context.SendActivity(locationsCardReply);

                    if (locations.Count == 1)
                    {
                        await dc.Prompt(Inputs.Confirm, resourceManager.SingleResultFound,
                            new PromptOptions()
                            {
                                RetryPromptString = resourceManager.ConfirmationInvalidResponse
                            });
                    }
                    else
                    {
                        await dc.Context.SendActivity(resourceManager.MultipleResultsFound);
                    }
                },
                async (dc, args, next) =>
                {
                    var locations = (List<Bing.Location>) dc.ActiveDialog.State[Outputs.Locations];

                    if (args is ConfirmResult result)
                    {
                        if (result.Confirmation)
                        {
                            await TryReverseGeocodeAddress(locations.First(), options, geoSpatialService);

                            if (requiredFields == LocationRequiredFields.None)
                            {
                                await dc.End(new Dictionary<string, object>
                                {
                                    {Outputs.SelectedLocation, locations.First()}
                                });
                            }

                            await dc.Replace(DialogIds.CompleteMissingRequiredFieldsDialog, dc.ActiveDialog.State);
                        }
                        else
                        {
                            await dc.Replace(DialogIds.LocationRetrieverRichDialog);
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
                                await dc.End(new Dictionary<string, object>
                                {
                                    {Outputs.SelectedLocation, locations[value - 1]}
                                });
                            }
                            else
                            {
                                await dc.Replace(DialogIds.CompleteMissingRequiredFieldsDialog, dc.ActiveDialog.State);
                            }
                        }
                        else if (StringComparer.OrdinalIgnoreCase.Equals(message, resourceManager.OtherComand))
                        {
                            dc.ActiveDialog.State[Outputs.SelectedLocation] = new Bing.Location();
                        }
                        else
                        {
                            await dc.Context.SendActivity(resourceManager.InvalidLocationResponse);
                            await dc.Replace(DialogIds.SelectAndConfirmLocationDialog, dc.ActiveDialog.State);
                        }
                    }
                }
            });

            Dialogs.Add(DialogIds.CompleteMissingRequiredFieldsDialog, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    Bing.Location selectedLocation;
                    if (args.ContainsKey(Outputs.SelectedLocation))
                    {
                        selectedLocation = (Bing.Location) args[Outputs.SelectedLocation];
                    }
                    else
                    {
                        selectedLocation = ((List<Bing.Location>) args[Outputs.Locations]).First();
                    }

                    dc.ActiveDialog.State[Outputs.SelectedLocation] = selectedLocation;

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
                        await dc.End(new Dictionary<string, object>
                        {
                            {Outputs.SelectedLocation, selectedLocation}
                        });
                    }
                },
                async (dc, args, next) =>
                {
                    var selectedLocation = (Bing.Location) dc.ActiveDialog.State[Outputs.SelectedLocation];

                    switch ((LocationRequiredFields)dc.ActiveDialog.State["CurrentMissingRequiredField"])
                    {
                        case LocationRequiredFields.StreetAddress:
                            selectedLocation.Address.AddressLine = ((TextResult) args).Text;
                            break;
                        case LocationRequiredFields.Locality:
                            selectedLocation.Address.Locality = ((TextResult) args).Text;
                            break;
                        case LocationRequiredFields.Region:
                            selectedLocation.Address.AdminDistrict = ((TextResult) args).Text;
                            break;
                        case LocationRequiredFields.PostalCode:
                            selectedLocation.Address.PostalCode = ((TextResult) args).Text;
                            break;
                        case LocationRequiredFields.Country:
                            selectedLocation.Address.CountryRegion = ((TextResult) args).Text;
                            break;
                    }

                    dc.ActiveDialog.State[Outputs.SelectedLocation] = selectedLocation;

                    await dc.Replace(DialogIds.CompleteMissingRequiredFieldsDialog, dc.ActiveDialog.State);
                }
            });

            Dialogs.Add(DialogIds.AddToFavoritesDialog, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    // If this is our first time through then get the selected location
                    // passed to us from the parent location dialog and store it in dialog state
                    if (args.ContainsKey(Outputs.SelectedLocation))
                        dc.ActiveDialog.State[Outputs.SelectedLocation] = args[Outputs.SelectedLocation];

                    // If we have a value for addToFavoritesConfirmed then we don't need to confirm again
                    // we need to skip to prompting for the name again instead
                    if (!dc.ActiveDialog.State.ContainsKey("addToFavoritesConfirmed"))
                    {
                        Bing.Location selectedLocation =
                            (Bing.Location) dc.ActiveDialog.State[Outputs.SelectedLocation];
                        // no capacity to add to favorites in the first place!
                        // OR the location is already marked as favorite
                        if (await favoritesManager.MaxCapacityReached(dc.Context)
                            || await favoritesManager.IsFavorite(dc.Context, selectedLocation))
                        {
                            await dc.End(new Dictionary<string, object>
                            {
                                {Outputs.SelectedLocation, selectedLocation}
                            });
                        }
                        else
                        {
                            await dc.Prompt(Inputs.Confirm, resourceManager.AddToFavoritesAsk, new PromptOptions()
                            {
                                RetryPromptString = resourceManager.AddToFavoritesRetry
                            });
                        }
                    }
                },
                async (dc, args, next) =>
                {
                    bool addToFavoritesConfirmation;

                    if (!dc.ActiveDialog.State.ContainsKey("addToFavoritesConfirmed"))
                    {
                        addToFavoritesConfirmation = ((ConfirmResult) args).Confirmation;
                    }
                    else
                    {
                        addToFavoritesConfirmation = true;
                    }

                    if (addToFavoritesConfirmation)
                    {
                        await dc.Prompt(Inputs.Text, resourceManager.EnterNewFavoriteLocationName);
                    }
                    else
                    {
                        var selectedLocation =
                            (Bing.Location) dc.ActiveDialog.State[Outputs.SelectedLocation];
                        await dc.End(new Dictionary<string, object>
                        {
                            {Outputs.SelectedLocation, selectedLocation}
                        });
                    }
                },
                async (dc, args, next) =>
                {
                    var newFavoriteName = ((TextResult) args).Text;

                    if (await favoritesManager.IsFavoriteLocationName(dc.Context, newFavoriteName))
                    {
                        await dc.Context.SendActivity(string.Format(resourceManager.DuplicateFavoriteNameResponse,
                            newFavoriteName));
                        await dc.Replace(DialogIds.AddToFavoritesDialog, dc.ActiveDialog.State);
                    }
                    else
                    {
                        Bing.Location selectedLocation =
                            (Bing.Location) dc.ActiveDialog.State[Outputs.SelectedLocation];
                        await favoritesManager.Add(dc.Context,
                            new FavoriteLocation {Location = selectedLocation, Name = newFavoriteName});
                        await dc.Context.SendActivity(string.Format(resourceManager.FavoriteAddedConfirmation,
                            newFavoriteName));
                        await dc.End(new Dictionary<string, object>
                        {
                            {Outputs.SelectedLocation, selectedLocation}
                        });
                    }
                }
            });

            Dialogs.Add(DialogIds.ConfirmDeleteFromFavoritesDialog, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    var location = (FavoriteLocation)args[Outputs.SelectedLocation];
                    dc.ActiveDialog.State[Outputs.SelectedLocation] = location;

                    var confirmationAsk = string.Format(
                    resourceManager.DeleteFavoriteConfirmationAsk,
                    $"{location.Name}: {location.Location.GetFormattedAddress(resourceManager.AddressSeparator)}");

                    await dc.Prompt(Inputs.Confirm, confirmationAsk,
                        new PromptOptions
                        {
                            RetryPromptString = resourceManager.ConfirmationInvalidResponse
                        });
                },
                async (dc, args, next) =>
                {
                    var confirmationResult = (ConfirmResult)args;
                    var selectedLocation = (FavoriteLocation)dc.ActiveDialog.State[Outputs.SelectedLocation];

                    if(confirmationResult.Confirmation)
                    {
                        await favoritesManager.Delete(dc.Context, selectedLocation);
                        await dc.Context.SendActivity(string.Format(resourceManager.FavoriteDeletedConfirmation, selectedLocation.Name));
                        await dc.Replace(DialogIds.FavoriteLocationRetrieverDialog);
                    }
                    else
                    {
                        await dc.Context.SendActivity(
                            string.Format(resourceManager.DeleteFavoriteAbortion,
                            selectedLocation.Name));
                        await dc.Replace(DialogIds.FavoriteLocationRetrieverDialog);
                    }
                }
            });
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
            await dc.Prompt(Inputs.Text, string.Format(resourceManager.AskForPrefix, formattedAddress) +
                                         string.Format(resourceManager.AskForTemplate,
                                             requiredFieldPrompt));
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

