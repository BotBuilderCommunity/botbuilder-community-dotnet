# Bot Builder Community Extensions
The repository for the community driven Bot Builder Extensions. A collection of middleware, dialogs, helpers and more for the Bot Builder SDK.

## Installation

Each extension, such as middleware or recognizers, is available individually from NuGet. See each individual component description for installation details and links.

## Middleware

The following pieces of middleware are currently available;

* [Handle Activity Type Middleware](https://github.com/garypretty/botbuilder-community/tree/master/libraries/Bot.Builder.Community.Middleware.HandleActivityType) - Middleware component which allows you to respond to different types of incoming activities, e.g. send a greeting, or even filter out activities you do not care about altogether.

* [BestMatch Middleware](https://github.com/garypretty/botbuilder-community/tree/master/libraries/Bot.Builder.Community.Middleware.BestMatch) - A middleware implementation of the popular open source BestMatchDialog for v3 of the SDK. This piece of middleware will allow you to match a message receieved from a bot user against a list of strings and then carry out an appropriate action. Matching does not have to be exact and you can set the threshold as to how closely the message should match with an item in the list.

## Recognizers

* [Fuzzy Matching Recognizer](https://github.com/garypretty/botbuilder-community/tree/master/libraries/Bot.Builder.Community.Recognizers.FuzzyRecognizer) - A recognizer that allows you to use fuzzy matching to compare strings.  Useful in situations such as when a user make a spelling mistake etc. When the recognizer is used a list of matches, along with confidence scores, are returned.
