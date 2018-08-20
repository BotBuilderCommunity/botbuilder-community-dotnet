## Location Dialog for Bot Builder v4 .NET SDK

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community) | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.24-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Location/) |

### Description
This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This dialog is an implementation of the [Bot Builder Location dialog that Microsoft release for v3](https://www.github.com/microsoft/botbuilder-location) of the Bot Builder SDK.

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

