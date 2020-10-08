[![Build Status](https://dev.azure.com/BotBuilder-Community/dotnet/_apis/build/status/BotBuilderCommunity.botbuilder-community-dotnet?branchName=master)](https://dev.azure.com/BotBuilder-Community/dotnet/_build/latest?definitionId=1&branchName=master) [![Bot Builder Community NuGet](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/profiles/BotBuilderCommunity)

**Latest release: v4.8.6 (15/04/2020) - [Release Notes](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/releases/tag/v4.8.6)**

# Bot Builder Community - .NET Extensions

This repository is part of the Bot Builder Community Project and contains Extensions for the Bot Builder .NET SDK, including adapters, middleware, dialogs, helpers and more. Other repos within the Bot Builder Community Project exist for extensions for [JavaScript](https://github.com/BotBuilderCommunity/botbuilder-community-js), [Python](https://github.com/BotBuilderCommunity/botbuilder-community-python) and [tools](https://github.com/BotBuilderCommunity/botbuilder-community-tools) - you can find our other repos under [our GitHub organisation for the project](https://github.com/BotBuilderCommunity/).  

To see a list of current extensions available for the Bot Builder .NET SDK, use the links below to jump to a section.

  - [Installation](#installation)
  - [Contributing and Reporting Issues](#contributing-and-reporting-issues)
  - [Adapters](#adapters)
  - [Dialogs and Prompts](#dialogs-and-prompts)
  - [Middleware](#middleware)
  - [Recognizers](#recognizers)
  - [Storage](#storage)

## Installation

Each extension, such as middleware or recognizers, is available individually from NuGet. See each individual component description for installation details and links.

## Contributing and Reporting Issues

We welcome and encourage contributions to this project, in the form of bug fixes, enhancements or new extensions. Please fork the repo and raise a PR if you have something you would like us to review for inclusion.  If you want to discuss an idea first then the best way to do this right now is to raise a GitHub issue or reach out to one of us on Twitter.

## Adapters

The following adapters can be used to expose your bot on additional channels not supported by the Azure Bot Service, such as Alexa.

| Name | Description | Sample? | NuGet |
| ------ | ------ | ------ | ------ |
| [Alexa Adapter](libraries/Bot.Builder.Community.Adapters.Alexa) | An adapter to allow for Alexa Skills to be built using the Bot Builder SDK. Includes broad support for Alexa Skills capabilities, including devices with displays (Show / Spot), Alexa Cards, access to user profile data and the ability to send Progressive Responses. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Alexa%20Adapter%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Alexa/) |
| [Google Adapter](libraries/Bot.Builder.Community.Adapters.Google) | An adapter to allow for Google Actions to be built using the Bot Builder SDK. Includes broad support for Google Actions capabilities, including Cards and suggestion chips | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Google%20Adapter%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Google/) |
| [Twitter Adapter](libraries/Bot.Builder.Community.Adapters.Twitter) | An adapter that integrates Twitter Direct Messages with the Bot Builder. The adapter sets up the required webhooks and responds to CRC requests. The webhooks code is based on the work by Tweety with modifications to support the Premium tier of the Account Activity API. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Twitter%20Adapter%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Twitter/) |
| [RingCentral Adapter](libraries/Bot.Builder.Community.Adapters.RingCentral) | An adapter that integrates the RingCentral Platform API with the Bot Builder. The adapter sets up the required webhooks and endpoints to RingCentral requests. Supporting features such as human handoff, activity publishing and WhatsApp support. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/RingCentral%20Adapter%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.RingCentral/) |
| [Zoom Adapter](libraries/Bot.Builder.Community.Adapters.Zoom) | An adapter that accepts and hanldes Zoom app requests.  Specifically developed to handle Zoom chatbot app requests and allow for the sending of messages using Zoom Message Templates.  The adapter will also receieve any event subscribed to via Zoom, even those not chatbot related. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/develop/samples/Zoom%20Adapter%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Adapters.Zoom/) |
| [Infobip WhatsApp Adapter](libraries/Bot.Builder.Community.Adapters.InfoBip) | An adapter that accepts and hanldes WhatsApp app and SMS requests via Infobip, including support for various messages and template types. | | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/develop/libraries/Bot.Builder.Community.Adapters.Infobip/) |

## Dialogs and Prompts

The following dialogs are currently available;

| Name | Description | Sample | NuGet |
| ------ | ------ | ------ | ------ |
| [Bot Builder v4 Location Dialog](libraries/Bot.Builder.Community.Dialogs.Location) | An implemention for v4 of the Bot Build .NET SDK of the [Microsoft.Bot.Builder.Location dialog project built for Bot Builder v3](https://github.com/microsoft/botbuilder-location). An open-source location picker control for Microsoft Bot Framework powered by Azure or Bing Maps REST services. This control will allow a user to search for a location, with the ability to specify required fields and also store locations as favorites for the user. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Location%20Dialog%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Location/) |
| [Bot Builder ChoiceFlow](libraries/Bot.Builder.Community.Dialogs.ChoiceFlow) |This dialog allows you to provide the user with a series of guides choice prompts in turn (defined in a JSON file or as a collection of ChoiceFlowItem objects), similar to when calling a telephone line with a series of automated options. The dialog returns the user's last choice as a result from the dialog and optionally provide the user with a simple text response depending on which choice they land on. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/ChoiceFlow%20Dialog%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.ChoiceFlow/) |
| [Bot Builder v4 FormFlow](libraries/Bot.Builder.Community.Dialogs.FormFlow) | An implemention for v4 of the Bot Build .NET SDK of the [Microsoft.Bot.Builder.FormFlow dialog project built for Bot Builder v3](https://github.com/Microsoft/BotBuilder-V3/tree/master/CSharp/Library/Microsoft.Bot.Builder/FormFlow). FormFlow automatically generates the dialogs that are necessary to manage a guided conversation, based upon guidelines you specify. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Form%20Flow%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.FormFlow/) |
| [Bot Builder v4 Prompts](libraries/Bot.Builder.Community.Dialogs.Prompts) | A collection of Prompts for use with Bot Builder v4, providing the ability to prompt using Adaptive Cards and for recognizing currencies, age, distances and temperature. | | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Prompts/) |
| [Bot Builder v4 Luis Dialog](libraries/Bot.Builder.Community.Dialogs.Luis) | An implementation for v4 of the Bot Builder .NET SDK of the [Microsoft.Bot.Builder.Dialogs.LuisDialog built for Bot Builder V3](https://github.com/microsoft/BotBuilder-V3/tree/master/CSharp/Library/Microsoft.Bot.Builder/Luis) A dialog specialized to handle intents and entities from LUIS. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/Luis%20Dialog%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Luis/) |
| [Adaptive Dialogs - REST](libraries/Bot.Builder.Community.Dialogs.Adaptive.Rest) | Adaptive package containing additional actions to make calling REST endpoints easier when using Adaptive dialogs | | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Dialogs.Adaptive.Rest/) |

## Middleware

The following pieces of middleware are currently available;

| Name | Description | Sample? | NuGet |
| ------ | ------ | ------ | ------ |
| [Handle Activity Type Middleware](libraries/Bot.Builder.Community.Middleware.HandleActivityType) | Middleware component which allows you to respond to different types of incoming activities, e.g. send a greeting, or even filter out activities you do not care about altogether. | | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.HandleActivityType/) |
| [BestMatch Middleware](libraries/Bot.Builder.Community.Middleware.BestMatch) | A middleware implementation of the popular open source BestMatchDialog for v3 of the SDK. This piece of middleware will allow you to match a message received from a bot user against a list of strings and then carry out an appropriate action. Matching does not have to be exact and you can set the threshold as to how closely the message should match with an item in the list. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/master/samples/BestMatch%20Middleware%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.BestMatch/) |
| [Azure Active Directory Authentication Middleware](libraries/Bot.Builder.Community.Middleware.AzureAdAuthentication) | This middleware will allow your bot to authenticate with Azure AD. It was created to support integration with Microsoft Graph but it will work with any application that uses the OAuth 2.0 authorization code flow. | | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.AzureAdAuthentication/) |
| [Sentiment Analysis Middleware](libraries/Bot.Builder.Community.Middleware.SentimentAnalysis) | This middleware uses Cognitive Services Sentiment Analysis to identify the sentiment of each inbound message and make it available for your bot or other middleware component. | [Sample](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet/tree/develop/samples/Sentiment%20Middleware%20Sample) | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.SentimentAnalysis/) |
| [Spell Check Middleware](libraries/Bot.Builder.Community.Middleware.SpellCheck) | This middleware uses Cognitive Services Check to automatically correct inbound message text | | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Middleware.SpellCheck/) |

## Recognizers

The following recognizers are currently available;

| Name | Description | NuGet |
| ------ | ------ | ------ |
| [Fuzzy Matching Recognizer](libraries/Bot.Builder.Community.Recognizers.FuzzyRecognizer) | A recognizer that allows you to use fuzzy matching to compare strings.  Useful in situations such as when a user make a spelling mistake etc. When the recognizer is used a list of matches, along with confidence scores, are returned. | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Recognizers.FuzzyRecognizer/) |

## Storage

The following Storage implementations are currently available;

| Name | Description | NuGet |
| ------ | ------ | ------ |
| [Elasticsearch storage](libraries/Bot.Builder.Community.Storage.Elasticsearch) | Elasticsearch based storage for bots created using Microsoft Bot Builder SDK. | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Storage.Elasticsearch/) |
| [EntityFramework storage](libraries/Bot.Builder.Community.Storage.EntityFramework) | EntityFramework based storage and transcript store for bots created using Microsoft Bot Builder SDK. | [![NuGet version](https://img.shields.io/badge/NuGet-blue.svg)](https://www.nuget.org/packages/Bot.Builder.Community.Storage.EntityFramework/) |
