[![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community)

# Bot Builder Community - .NET Extensions

This repository is part of the Bot Builder Community Project and contains Bot Builder Extensions for the .NET SDK, including middleware, dialogs, helpers and more. Other repos within the Bot Builder Community Project exist for extensions for [JavaScript](https://github.com/BotBuilderCommunity/botbuilder-community-js), [Python](https://github.com/BotBuilderCommunity/botbuilder-community-python) and [tools](https://github.com/BotBuilderCommunity/botbuilder-community-tools) - you can find our other repos under [our GitHub organisation for the project](https://github.com/BotBuilderCommunity/).  

To see a list of current extensions available for the Bot Builder .NET SDK, use the links below to jump to a section.

* [C# Middleware](#middleware)
* [C# Dialogs & Prompts](#dialogs-and-prompts)
* [C# Recognizers](#recognizers)

## Installation

Each extension, such as middleware or recognizers, is available individually from NuGet. See each individual component description for installation details and links.

## Contributing and Reporting Issues

We welcome and encourage contributions to this project, in the form of bug fixes, enhancements or new extensions. Please fork the repo and raise a PR if you have something you would like us to review for inclusion.  If you want to discuss an idea first then the best way to do this right now is to raise a GitHub issue or reach out to one of us on Twitter.

## Middleware

The following pieces of middleware are currently available;

| Name | Description | NuGet |
| ------ | ------ | ------ |
| [Handle Activity Type Middleware](libraries/Bot.Builder.Community.Middleware.HandleActivityType) | Middleware component which allows you to respond to different types of incoming activities, e.g. send a greeting, or even filter out activities you do not care about altogether. | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.76-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.HandleActivityType/) |
| [BestMatch Middleware](libraries/Bot.Builder.Community.Middleware.BestMatch) | A middleware implementation of the popular open source BestMatchDialog for v3 of the SDK. This piece of middleware will allow you to match a message receieved from a bot user against a list of strings and then carry out an appropriate action. Matching does not have to be exact and you can set the threshold as to how closely the message should match with an item in the list. | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.76-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.BestMatch/) |
| [Azure Active Directory Authentication Middleware](libraries/Bot.Builder.Community.Middleware.AzureAdAuthentication) | This middleware will allow your bot to authenticate with Azure AD. It was created to support integration with Microsoft Graph but it will work with any application that uses the OAuth 2.0 authorization code flow. | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.76-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.AzureAdAuthentication/) |
| [Sentiment Analysis Middleware](libraries/Bot.Builder.Community.Middleware.SentimentAnalysis) | This middleware uses Cognitive Services Sentiment Analysis to identify the sentiment of each inbound message and make it available for your bot or other middleware component. | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.76-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.SentimentAnalysis/) |
| [Spell Check Middleware](libraries/Bot.Builder.Community.Middleware.SpellCheck) | This middleware uses Cognitive Services Check to automatically correct inbound message text | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.76-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.SpellCheck/) |

## Dialogs and Prompts

The following dialogs are currently available;

| Name | Description | NuGet |
| ------ | ------ | ------ |
| [Bot Builder v4 Prompts](libraries/Bot.Builder.Community.Dialogs.Prompts) | A collection of Prompts for use with Bot Builder v4, providing the ability to prompt for and recognize currencies, age, distances and temperature. | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.79-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Prompts/) |
| [Bot Builder v4 Location Dialog](libraries/Bot.Builder.Community.Dialogs.Location) | An implemention for v4 of the Bot Build .NET SDK of the [Microsoft.Bot.Builder.Location dialog project built for Bot Builder v3](https://github.com/microsoft/botbuilder-location). An open-source location picker control for Microsoft Bot Framework powered by Azure or Bing Maps REST services. This control will allow a user to search for a location, with the ability to specify required fields and also store locations as favorites for the user. | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.76-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Location/) |

## Recognizers

The following recognizers are currently available;

| Name | Description | NuGet |
| ------ | ------ | ------ |
| [Fuzzy Matching Recognizer](libraries/Bot.Builder.Community.Recognizers.FuzzyRecognizer) | A recognizer that allows you to use fuzzy matching to compare strings.  Useful in situations such as when a user make a spelling mistake etc. When the recognizer is used a list of matches, along with confidence scores, are returned. | [![NuGet version](https://img.shields.io/badge/NuGet-1.0.76-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Recognizers.FuzzyRecognizer/) |

