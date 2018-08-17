[![Build status](https://ci.appveyor.com/api/projects/status/b9123gl3kih8x9cb?svg=true)](https://ci.appveyor.com/project/garypretty/botbuilder-community)

# Bot Builder Community Extensions
The repository for the community driven Bot Builder Extensions. A collection of middleware, dialogs, helpers and more for the Bot Builder SDK.

## Installation

Each extension, such as middleware or recognizers, is available individually from NuGet. See each individual component description for installation details and links.

## Middleware

The following pieces of middleware are currently available;

* [Handle Activity Type Middleware](libraries/Bot.Builder.Community.Middleware.HandleActivityType) - Middleware component which allows you to respond to different types of incoming activities, e.g. send a greeting, or even filter out activities you do not care about altogether.

* [BestMatch Middleware](libraries/Bot.Builder.Community.Middleware.BestMatch) - A middleware implementation of the popular open source BestMatchDialog for v3 of the SDK. This piece of middleware will allow you to match a message receieved from a bot user against a list of strings and then carry out an appropriate action. Matching does not have to be exact and you can set the threshold as to how closely the message should match with an item in the list.

* [Azure Active Directory Authentication Middleware](libraries/Bot.Builder.Community.Middleware.AzureAdAuthentication) - This middleware will allow your bot to authenticate with Azure AD. It was created to support integration with Microsoft Graph but it will work with any application that uses the OAuth 2.0 authorization code flow.

* [Sentiment Analysis Middleware](libraries/Bot.Builder.Community.Middleware.SentimentAnalysis) - This middleware uses Cognitive Services Sentiment Analysis to identify the sentiment of each inbound message and make it available for your bot or other middleware component.

* [Spell Check Middleware](libraries/Bot.Builder.Community.Middleware.SpellCheck) - This middleware uses Cognitive Services Check to automatically correct inbound message text.

* [Typing Middleware](libraries/Bot.Builder.Community.Middleware.Typing) - This middleware will show a 'typing' event whenever a long running operation is occurring in your bot or other middeware components in the pipeline, providing a visual cue to the user that your bot is doing something.

## Recognizers

* [Fuzzy Matching Recognizer](libraries/Bot.Builder.Community.Recognizers.FuzzyRecognizer) - A recognizer that allows you to use fuzzy matching to compare strings.  Useful in situations such as when a user make a spelling mistake etc. When the recognizer is used a list of matches, along with confidence scores, are returned.

