## Location Dialog for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.24-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Location/) |

### Description
This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This dialog is an implementation based on the [Bot Builder Location dialog that Microsoft release for v3](https://www.github.com/microsoft/botbuilder-location) of the Bot Builder SDK.

The dialog, which has 100% feature parity with the v3 dialog, makes it simple for your to prompt your bot's users for a location. It has the following features.

* Address look up and validation using the REST services of Azure Maps or Bing Maps depending on which APOI key you use in your bot.
* User location returned as strongly-typed object complying with schema.org.
* Address disambiguation when more than one address is found.
* Support for declaring required location fields.
* Support for FB Messenger's location picker GUI dialog.
* Customizable dialog strings.

### Installation

Available via NuGet package [Bot.Builder.Community.Dialogs.Location](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Location/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Dialogs.Location
```

### Usage

To use the dialog, add it to your DialogSet as shown below. 

```cs

Dialogs.Add(LocationDialog.MainDialogId,
                new LocationDialog("<YOUR-API-KEY-FOR-BING-OR-AZURE-MAPS",
                    "Please enter a location", 
                    useAzureMaps: false, 
                    requiredFields: LocationRequiredFields.StreetAddress | LocationRequiredFields.PostalCode, 
                    options: LocationOptions.SkipFavorites | LocationOptions.SkipFinalConfirmation 
                    ));

```

The following settings can be applied when adding your dialog to the DialogSet.

* **ApiKey** - Required - This should be the API key for the mapping provider you are using
* **Prompt** - Required - The initial prompt shown to the user. e.g. "Please enter your post code"
* **UseAzureMaps** - Optional - This defaults to true, in which case Azure maps will be used and you should provide an Azure Maps API key. If you set this to false then Bing Maps will be used and you should use a Bing Maps API key.
* **LocationRequiredFields** - Optional - Here you can pass in a list of required fields which the use will need to populate if they are empty when a location is found using the initial search. In the example above I have specified Street Address and Post Code are required.
* **Options** - Optional - Various options to control how the dialog works. Including the abiity to use / not use the favorites functionality, skipping the final confirmation and reverse geocoding of addresses.
* **LocationResourceManager** - Optional - Here you can provde an implementation of the LocationResourceManager class, which allows you to override the default strings used by the dialog.

In your bot code, when you want to hand off to the Location dialog you can use dc.Begin to do this, as shown in the below example using a Waterfall dialog. When returning, the dialog will return args of type LocationDialogResult, which you can also see below.

```cs

Dialogs.Add("YourBotsDialog", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                        await dc.Begin(LocationDialog.MainDialogId);
                },
                async (dc, args, next) =>
                {
                    if (args is LocationDialogResult locationDialogResult)
                    {
                        await dc.Context.SendActivity($"Location found: {locationDialogResult.SelectedLocation.Address}");
                        await dc.End();
                    }
                    else
                    {
                        await dc.Context.SendActivity($"No location found");
                        await dc.End();
                    }
                }
            });

```

