## Luis Dialog

### Build status
| Branch | Status | Recommended NuGet package version |
| ------ | ------ | ------ |
| master | | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.0-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Luis/) |

### Description
This is part of the [Bot Builder Community Extensions](https://github.com/BotBuilderCommunity) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

This library is a netstandard2.0 port of [Microsoft.Bot.Builder.Luis](https://github.com/microsoft/BotBuilder-V3/tree/master/CSharp/Library/Microsoft.Bot.Builder/Luis) and the [Microsoft.Bot.Builder.Dialogs.LuisDialog](https://github.com/microsoft/BotBuilder-V3/blob/master/CSharp/Library/Microsoft.Bot.Builder/Dialogs/LuisDialog.cs)  The Bot Builder V3 LuisDialog documentation is useful for this library as well, just take note of the namespace changes inheritence changes.  **Bot.Builder.Community.Dialogs.Luis.LuisDialog** inherits from ComponentDialog.

### Installation

Available via NuGet package [Bot.Builder.Community.Dialogs.Luis](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Luis/)

Install into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Dialogs.Luis
```

### Sample

A basic sample for using this component can be found [here](../../samples/Luis%20Dialog%20Sample).

### References
Take note of namespace changes from **Microsoft.Bot.Builder.Luis** to **Bot.Builder.Community.Dialogs.Luis**

* [Basic features of LuisDialog](https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-luis-dialogs)  (Note: this is a V3 docs page, but many of the concepts for the V3 Luis Dialog also apply to the V4 Luis Dialog.)

### License

Licensed under the MIT License.

